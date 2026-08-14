import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, finalize } from 'rxjs';
import { AuthService } from '../services/auth.service';

// Tracks whether a refresh request is already running.
let isRefreshing = false;

// Holds the new token so other failed requests can wait for it.
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  const publicEndpoints = [
    '/Auth/login',
    '/Auth/register',
    '/Auth/request-email-verification',
    '/Auth/verify-email',
    '/Auth/refresh',
  ];

  // Public auth requests don't need a JWT.
  if (publicEndpoints.some((endpoint) => req.url.includes(endpoint))) {
    return next(req);
  }

  const token = authService.getAccessToken();

  if (!token) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authReq).pipe(
    catchError((error) => {
      // Only handle authentication failures.
      if (error.status !== 401) {
        return throwError(() => error);
      }

      // If another request is already refreshing,
      // wait for that refresh to finish instead of starting another one.
      if (isRefreshing) {
        return refreshTokenSubject.pipe(
          filter((newToken) => newToken !== null),
          take(1),
          switchMap((newToken) => {
            const retryRequest = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`,
              },
            });

            return next(retryRequest);
          }),
        );
      }

      // This request becomes responsible for refreshing the token.
      isRefreshing = true;
      refreshTokenSubject.next(null);

      return authService.refreshAccessToken().pipe(
        switchMap((response) => {
          // Save the new access token.
          authService.setAccessToken(response.accessToken);

          // Let waiting requests know that refresh succeeded.
          refreshTokenSubject.next(response.accessToken);

          // Retry the original failed request.
          const retryRequest = req.clone({
            setHeaders: {
              Authorization: `Bearer ${response.accessToken}`,
            },
          });

          return next(retryRequest);
        }),
        catchError((refreshError) => {
          // Refresh failed → user session is no longer valid.
          authService.clearTokens();
          refreshTokenSubject.next(null);

          return throwError(() => refreshError);
        }),
        finalize(() => {
          // Always release the refresh lock,
          // whether refresh succeeds or fails.
          isRefreshing = false;
        }),
      );
    }),
  );
};

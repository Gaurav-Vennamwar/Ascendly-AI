import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

type SeoRouteData = {
  title?: string;
  description?: string;
  robots?: 'index, follow' | 'noindex, nofollow';
};

@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly siteUrl = 'https://www.ascendlyai.in';
  private readonly defaultTitle = 'Ascendly AI | Build a Career That Gets Noticed';
  private readonly defaultDescription =
    'Ascendly AI helps developers optimize resumes for ATS, prepare for interviews, and build a focused career roadmap.';
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly document = inject(DOCUMENT);

  initialize(): void {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.updateForRoute());

    this.updateForRoute();
  }

  private updateForRoute(): void {
    const routeData = this.getDeepestRoute(this.activatedRoute).snapshot.data as SeoRouteData;
    const title = routeData.title ? `${routeData.title} | Ascendly AI` : this.defaultTitle;
    const description = routeData.description ?? this.defaultDescription;
    const robots = routeData.robots ?? 'index, follow';
    const canonicalUrl = new URL(this.router.url.split('?')[0].split('#')[0], `${this.siteUrl}/`).toString();

    this.title.setTitle(title);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ name: 'robots', content: robots });
    this.meta.updateTag({ property: 'og:title', content: title });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:url', content: canonicalUrl });
    this.meta.updateTag({ name: 'twitter:title', content: title });
    this.meta.updateTag({ name: 'twitter:description', content: description });
    this.setCanonical(canonicalUrl);
  }

  private getDeepestRoute(route: ActivatedRoute): ActivatedRoute {
    while (route.firstChild) {
      route = route.firstChild;
    }
    return route;
  }

  private setCanonical(url: string): void {
    let link = this.document.querySelector<HTMLLinkElement>("link[rel='canonical']");
    if (!link) {
      link = this.document.createElement('link');
      link.rel = 'canonical';
      this.document.head.appendChild(link);
    }
    link.href = url;
  }
}

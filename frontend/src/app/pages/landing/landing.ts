import { Component, AfterViewInit, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Navbar } from '../../shared/components/navbar/navbar';
import { RouterLink } from '@angular/router';

declare const gsap: any;
declare const ScrollTrigger: any;

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [Navbar, RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements AfterViewInit {
  private platformId = inject(PLATFORM_ID);

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    // Initialize GSAP scroll animations
    if (typeof gsap !== 'undefined') {
      if (typeof ScrollTrigger !== 'undefined') {
        gsap.registerPlugin(ScrollTrigger);
      }

      // Hero Elements Entrance
      gsap.from('.hero-left > *', {
        opacity: 0,
        y: 28,
        duration: 0.9,
        stagger: 0.12,
        ease: 'power3.out'
      });

      gsap.from('.workbench-terminal', {
        opacity: 0,
        y: 35,
        scale: 0.97,
        duration: 1.1,
        ease: 'power3.out',
        delay: 0.2
      });

      // Feature Cards staggered reveal on scroll
      if (typeof ScrollTrigger !== 'undefined') {
        gsap.from('.feature-card', {
          scrollTrigger: {
            trigger: '.features',
            start: 'top 85%'
          },
          opacity: 0,
          y: 30,
          duration: 0.7,
          stagger: 0.12,
          ease: 'power2.out'
        });

        // Workflow Steps reveal
        gsap.from('.workflow-step-card', {
          scrollTrigger: {
            trigger: '.workflow',
            start: 'top 85%'
          },
          opacity: 0,
          y: 25,
          duration: 0.6,
          stagger: 0.1,
          ease: 'power2.out'
        });

        // Stats Cards reveal
        gsap.from('.stat-card', {
          scrollTrigger: {
            trigger: '.stats',
            start: 'top 90%'
          },
          opacity: 0,
          scale: 0.95,
          duration: 0.6,
          stagger: 0.1,
          ease: 'back.out(1.5)'
        });
      }
    }
  }
}

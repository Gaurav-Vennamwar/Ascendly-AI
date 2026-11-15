import { Component, AfterViewInit, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Navbar } from '../../shared/components/navbar/navbar';
import { RouterLink } from '@angular/router';

declare const gsap: any;

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

    // Smooth entry for hero elements without hiding scroll sections
    if (typeof gsap !== 'undefined') {
      gsap.from('.hero-left > *', {
        opacity: 0,
        y: 20,
        duration: 0.8,
        stagger: 0.1,
        ease: 'power3.out'
      });

      gsap.from('.workbench-terminal', {
        opacity: 0,
        y: 25,
        duration: 0.9,
        ease: 'power3.out',
        delay: 0.15
      });
    }
  }
}

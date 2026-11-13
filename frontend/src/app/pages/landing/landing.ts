import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit
} from '@angular/core';

import { Navbar } from '../../shared/components/navbar/navbar';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [Navbar, RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements AfterViewInit {

  @ViewChild('network')
  network!: ElementRef<HTMLDivElement>;

  @ViewChild('particles')
  particles!: ElementRef<HTMLDivElement>;

  ngAfterViewInit(): void {
    // The landing environment is intentionally static: the content and product preview lead.
  }

  private createParticles() {

    const container = this.particles.nativeElement;

    for (let i = 0; i < 35; i++) {

      const particle = document.createElement('div');

      particle.className = 'particle';

      const size = Math.random() * 3 + 1;

      particle.style.width = `${size}px`;
      particle.style.height = `${size}px`;

      particle.style.left = `${Math.random() * 100}%`;

      particle.style.background = `rgba(255,255,255,${Math.random() * .34 + .16})`;

      particle.style.animationDuration =
        `${Math.random() * 15 + 10}s`;

      particle.style.animationDelay =
        `-${Math.random() * 15}s`;

      container.appendChild(particle);

    }

  }

  private enableParallax() {

    document.addEventListener('mousemove', (event) => {

      const x =
        event.clientX / window.innerWidth - .5;

      const y =
        event.clientY / window.innerHeight - .5;

      this.network.nativeElement.style.transform =
        `translate(${x * 18}px, ${y * 12}px)`;

    });

  }

}

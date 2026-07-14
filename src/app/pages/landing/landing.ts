import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit
} from '@angular/core';

import { Navbar } from '../../shared/components/navbar/navbar';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [Navbar],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements AfterViewInit {

  @ViewChild('network')
  network!: ElementRef<HTMLDivElement>;

  @ViewChild('particles')
  particles!: ElementRef<HTMLDivElement>;

  ngAfterViewInit(): void {

    this.createParticles();
    this.enableParallax();

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

      particle.style.background =
        Math.random() > 0.5
          ? `rgba(34,211,238,${Math.random() * .5 + .2})`
          : `rgba(99,102,241,${Math.random() * .5 + .2})`;

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
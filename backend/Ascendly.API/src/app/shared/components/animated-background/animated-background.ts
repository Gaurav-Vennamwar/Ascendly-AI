import { Component } from '@angular/core';

@Component({
  selector: 'app-animated-background',
  standalone: true,
  templateUrl: './animated-background.html',
  styleUrl: './animated-background.scss'
})
export class AnimatedBackgroundComponent {

  particles = Array.from({ length: 70 }, () => ({
    x: Math.random() * 100,
    y: Math.random() * 100,
    delay: Math.random() * 5,
    duration: 8 + Math.random() * 10
  }));

}

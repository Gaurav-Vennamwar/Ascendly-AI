import { signal } from '@angular/core';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AnimatedBackgroundComponent } from './shared/components/animated-background/animated-background';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AnimatedBackgroundComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
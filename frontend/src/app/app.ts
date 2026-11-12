import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AnimatedBackgroundComponent } from './shared/components/animated-background/animated-background';
import { SeoService } from './core/services/seo.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AnimatedBackgroundComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly seo = inject(SeoService);

  ngOnInit(): void {
    this.seo.initialize();
  }
}

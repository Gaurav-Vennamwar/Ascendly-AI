import { Component, input } from '@angular/core';

@Component({
  selector: 'app-primary-button',
  standalone: true,
  templateUrl: './primary-button.html',
  styleUrl: './primary-button.scss'
})
export class PrimaryButton {
  label = input.required<string>();
  variant = input<'primary' | 'secondary' | 'ghost'>('primary');
  icon = input<string>('');
  type = input<'button' | 'submit'>('button');
}

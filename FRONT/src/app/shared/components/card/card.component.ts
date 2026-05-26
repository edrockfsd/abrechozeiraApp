import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type CardVariant = 'primary' | 'accent' | 'warn' | 'neutral';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.scss']
})
export class CardComponent {
  @Input() title = '';
  @Input() variant: CardVariant = 'neutral';
}


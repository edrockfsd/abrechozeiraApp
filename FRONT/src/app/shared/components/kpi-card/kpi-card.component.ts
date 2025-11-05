import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type KpiVariant = 'primary' | 'accent' | 'warn' | 'neutral';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kpi-card.component.html',
  styleUrls: ['./kpi-card.component.scss']
})
export class KpiCardComponent {
  @Input() title = '';
  @Input() value: string | number = '';
  @Input() icon: string = 'insights'; // material icon name
  @Input() variant: KpiVariant = 'neutral';
}


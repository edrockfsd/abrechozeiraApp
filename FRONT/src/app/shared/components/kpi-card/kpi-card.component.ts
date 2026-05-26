import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type KpiVariant = 'blue' | 'purple' | 'green' | 'pink' | 'orange' | 'red' | 'primary' | 'accent' | 'warn' | 'neutral';

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
  @Input() icon: string = 'insights';
  @Input() variant: KpiVariant = 'blue';

  get iconColorClass(): string {
    // Map old variants to new colors
    const variantMap: Record<string, string> = {
      'primary': 'blue',
      'accent': 'purple',
      'warn': 'red',
      'neutral': 'blue'
    };
    return variantMap[this.variant] || this.variant;
  }
}

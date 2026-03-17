import {
  Component,
  ChangeDetectionStrategy,
  input,
  computed,
} from "@angular/core";

export interface BarChartItem {
  label: string;
  value: number;
}

@Component({
  selector: "app-bar-chart",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="bar-chart">
      @if (title()) {
        <h4 class="bar-chart__title">{{ title() }}</h4>
      }
      @if (normalizedItems().length === 0) {
        <div class="bar-chart__empty">No data available</div>
      } @else {
        <div class="bar-chart__bars">
          @for (item of normalizedItems(); track item.label) {
            <div class="bar-chart__row">
              <span class="bar-chart__label" [title]="item.label">
                {{ item.label }}
              </span>
              <div class="bar-chart__bar-wrap">
                <div
                  class="bar-chart__bar"
                  [style.width.%]="item.percent"
                  [style.background]="barColor()"
                ></div>
              </div>
              <span class="bar-chart__value">{{ item.value }}</span>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [
    `
      .bar-chart {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .bar-chart__title {
        font-size: 1rem;
        font-weight: 600;
        margin: 0;
      }
      .bar-chart__empty {
        text-align: center;
        padding: 1.5rem;
        color: #999;
        font-size: 0.9rem;
      }
      .bar-chart__bars {
        display: flex;
        flex-direction: column;
        gap: 0.4rem;
      }
      .bar-chart__row {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }
      .bar-chart__label {
        width: 120px;
        font-size: 0.85rem;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        flex-shrink: 0;
        text-align: right;
      }
      .bar-chart__bar-wrap {
        flex: 1;
        height: 22px;
        background: #f1f5f9;
        border-radius: 4px;
        overflow: hidden;
      }
      .bar-chart__bar {
        height: 100%;
        border-radius: 4px;
        transition: width 0.3s ease;
        min-width: 2px;
      }
      .bar-chart__value {
        width: 48px;
        font-size: 0.85rem;
        font-weight: 500;
        text-align: right;
        flex-shrink: 0;
      }
    `,
  ],
})
export class BarChartComponent {
  readonly data = input<BarChartItem[]>([]);
  readonly title = input<string>("");
  readonly barColor = input<string>("#1db954");

  protected readonly normalizedItems = computed(() => {
    const items: BarChartItem[] = this.data();
    if (items.length === 0) return [];
    const maxVal = Math.max(...items.map((i: BarChartItem) => i.value), 1);
    return items.map((item: BarChartItem) => ({
      ...item,
      percent: (item.value / maxVal) * 100,
    }));
  });
}

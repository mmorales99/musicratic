import {
  Component,
  ChangeDetectionStrategy,
  input,
  computed,
} from "@angular/core";

export interface AreaChartPoint {
  label: string;
  value: number;
}

@Component({
  selector: "app-area-chart",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="area-chart">
      @if (title()) {
        <h4 class="area-chart__title">{{ title() }}</h4>
      }
      @if (data().length === 0) {
        <div class="area-chart__empty">No data available</div>
      } @else {
        <div class="area-chart__container">
          <div class="area-chart__grid">
            @for (point of normalizedPoints(); track point.label) {
              <div class="area-chart__column" [title]="point.label + ': ' + point.value">
                <div
                  class="area-chart__bar"
                  [style.height.%]="point.percent"
                  [style.background]="fillColor()"
                ></div>
                <span class="area-chart__label">{{ point.shortLabel }}</span>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .area-chart {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .area-chart__title {
        font-size: 1rem;
        font-weight: 600;
        margin: 0;
      }
      .area-chart__empty {
        text-align: center;
        padding: 1.5rem;
        color: #999;
        font-size: 0.9rem;
      }
      .area-chart__container {
        width: 100%;
        overflow-x: auto;
      }
      .area-chart__grid {
        display: flex;
        align-items: flex-end;
        gap: 3px;
        height: 160px;
        min-width: 100%;
      }
      .area-chart__column {
        flex: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: flex-end;
        height: 100%;
        min-width: 24px;
      }
      .area-chart__bar {
        width: 100%;
        border-radius: 3px 3px 0 0;
        transition: height 0.3s ease;
        min-height: 2px;
        opacity: 0.8;
      }
      .area-chart__label {
        font-size: 0.65rem;
        color: #888;
        text-align: center;
        white-space: nowrap;
        margin-top: 4px;
      }
    `,
  ],
})
export class AreaChartComponent {
  readonly data = input<AreaChartPoint[]>([]);
  readonly title = input<string>("");
  readonly fillColor = input<string>("#6366f1");

  protected readonly normalizedPoints = computed(() => {
    const items: AreaChartPoint[] = this.data();
    if (items.length === 0) return [];
    const maxVal = Math.max(...items.map((i: AreaChartPoint) => i.value), 1);
    return items.map((item: AreaChartPoint) => ({
      ...item,
      shortLabel: item.label.slice(5),
      percent: (item.value / maxVal) * 100,
    }));
  });
}

import {
  Component,
  ChangeDetectionStrategy,
  input,
  computed,
} from "@angular/core";

export interface LineChartPoint {
  label: string;
  values: { name: string; value: number }[];
}

export interface LineChartSeries {
  name: string;
  color: string;
}

@Component({
  selector: "app-line-chart",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="line-chart">
      @if (title()) {
        <h4 class="line-chart__title">{{ title() }}</h4>
      }
      @if (data().length === 0) {
        <div class="line-chart__empty">No data available</div>
      } @else {
        <!-- Legend -->
        <div class="line-chart__legend">
          @for (s of series(); track s.name) {
            <span class="line-chart__legend-item">
              <span
                class="line-chart__legend-dot"
                [style.background]="s.color"
              ></span>
              {{ s.name }}
            </span>
          }
        </div>
        <!-- CSS-based area chart approximation -->
        <div class="line-chart__container">
          <div class="line-chart__grid">
            @for (point of normalizedPoints(); track point.label) {
              <div class="line-chart__column">
                @for (bar of point.bars; track bar.name) {
                  <div
                    class="line-chart__column-bar"
                    [style.height.%]="bar.percent"
                    [style.background]="bar.color"
                    [title]="bar.name + ': ' + bar.value"
                  ></div>
                }
                <span class="line-chart__column-label">
                  {{ point.shortLabel }}
                </span>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .line-chart {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .line-chart__title {
        font-size: 1rem;
        font-weight: 600;
        margin: 0;
      }
      .line-chart__empty {
        text-align: center;
        padding: 1.5rem;
        color: #999;
        font-size: 0.9rem;
      }
      .line-chart__legend {
        display: flex;
        gap: 1rem;
        font-size: 0.85rem;
      }
      .line-chart__legend-item {
        display: flex;
        align-items: center;
        gap: 0.3rem;
      }
      .line-chart__legend-dot {
        width: 10px;
        height: 10px;
        border-radius: 50%;
        flex-shrink: 0;
      }
      .line-chart__container {
        width: 100%;
        overflow-x: auto;
      }
      .line-chart__grid {
        display: flex;
        align-items: flex-end;
        gap: 4px;
        height: 180px;
        min-width: 100%;
      }
      .line-chart__column {
        flex: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: flex-end;
        gap: 2px;
        height: 100%;
        min-width: 28px;
      }
      .line-chart__column-bar {
        width: 100%;
        border-radius: 2px 2px 0 0;
        transition: height 0.3s ease;
        min-height: 2px;
      }
      .line-chart__column-label {
        font-size: 0.65rem;
        color: #888;
        text-align: center;
        white-space: nowrap;
        margin-top: 4px;
      }
    `,
  ],
})
export class LineChartComponent {
  readonly data = input<LineChartPoint[]>([]);
  readonly series = input<LineChartSeries[]>([]);
  readonly title = input<string>("");

  protected readonly normalizedPoints = computed(() => {
    const points = this.data();
    const seriesList = this.series();
    if (points.length === 0) return [];

    const allValues = points.flatMap(
      (p: LineChartPoint) => p.values.map((v: { name: string; value: number }) => v.value),
    );
    const maxVal = Math.max(...allValues, 1);

    const seriesColorMap = new Map(
      seriesList.map((s: LineChartSeries) => [s.name, s.color] as const),
    );

    return points.map((point: LineChartPoint) => ({
      label: point.label,
      shortLabel: point.label.slice(5), // trim year "2026-" → "03-17"
      bars: point.values.map((v: { name: string; value: number }) => ({
        name: v.name,
        value: v.value,
        percent: (v.value / maxVal) * 100,
        color: seriesColorMap.get(v.name) ?? "#94a3b8",
      })),
    }));
  });
}

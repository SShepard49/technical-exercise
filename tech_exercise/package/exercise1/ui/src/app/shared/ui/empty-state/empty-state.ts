import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="empty-state" role="status">
      <span class="material-icons icon" aria-hidden="true">{{ icon() }}</span>
      <h3 class="title">{{ title() }}</h3>
      @if (description(); as desc) {
        <p class="description">{{ desc }}</p>
      }
      <ng-content></ng-content>
    </div>
  `,
  styleUrl: './empty-state.scss',
})
export class EmptyState {
  readonly icon = input<string>('inbox');
  readonly title = input.required<string>();
  readonly description = input<string | null>(null);
}

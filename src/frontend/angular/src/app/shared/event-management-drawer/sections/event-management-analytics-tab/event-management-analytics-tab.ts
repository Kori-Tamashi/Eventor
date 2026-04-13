import { Component, signal } from '@angular/core';
import { DrawerCard } from '../../../drawer-card/drawer-card';

type EventManagementAnalyticsMetric = {
  label: string;
  value: string;
};

@Component({
  selector: 'app-event-management-analytics-tab',
  standalone: true,
  imports: [DrawerCard],
  templateUrl: './event-management-analytics-tab.html',
  styleUrl: './event-management-analytics-tab.scss',
})
export class EventManagementAnalyticsTab {
  readonly generalMetrics = signal<EventManagementAnalyticsMetric[]>([
    { label: 'Количество участников', value: '7/150' },
    { label: 'Рейтинг', value: '10' },
    { label: 'Стоимость мероприятия', value: '187500' },
    { label: 'Средняя стоимость дня', value: '62500' },
    { label: 'Возможность расчета цены', value: 'Расчет возможен' },
    { label: 'N-мерность мероприятия', value: '3' },
    { label: 'Осталось дней до начала', value: 'Мероприятие завершилось' },
  ]);

  readonly pricingMetrics = signal<EventManagementAnalyticsMetric[]>([
    { label: 'Фундаментальная цена', value: '13160.53' },
    { label: 'Фундаментальная цена (с привилегиями)', value: '16707.25' },
    { label: 'Относительная разница фундаментальных цен', value: '26.95%' },
    { label: 'Средняя цена дня', value: '15430.88' },
    { label: 'Средняя цена дня (с привилегиями)', value: '19589.45' },
    { label: 'Цена мероприятия', value: '46292.65' },
    { label: 'Цена мероприятия (с привилегиями)', value: '58768.35' },
  ]);

  readonly profitMetrics = signal<EventManagementAnalyticsMetric[]>([
    { label: 'Расходы', value: '187500' },
    { label: 'Доход', value: '215625' },
    { label: 'Фактическая прибыль', value: '28125' },
    { label: 'Теоретическая прибыль', value: '28125' },
    { label: 'Максимальная наценка', value: '71.59%' },
    { label: 'Минимальное количество участников', value: '7' },
  ]);
}

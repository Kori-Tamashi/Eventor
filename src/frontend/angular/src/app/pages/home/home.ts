import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

type MockRow = {
  id: number;
  name: string;
  date: string;
  count: number;
  delay: number;
  status: 'active' | 'done' | 'pending';
  statusLabel: string;
};

type Feature = {
  icon: string;
  title: string;
  desc: string;
  delay: number;
};

type Stat = {
  value: string;
  label: string;
  delay: number;
};

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ButtonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  readonly mockRows: MockRow[] = [
    { id: 1, name: 'Весенний марафон', date: '01.04.2026', count: 148, delay: 350, status: 'active', statusLabel: 'Активно' },
    { id: 2, name: 'Турнир по lethal company', date: '20.04.2026', count: 64, delay: 420, status: 'pending', statusLabel: 'Скоро' },
    { id: 3, name: 'слава кпсс - оксимирон', date: '06.08.2017', count: 320, delay: 490, status: 'done', statusLabel: 'Завершено' },
    { id: 4, name: 'Турнир по доте', date: '18.05.2026', count: 210, delay: 560, status: 'pending', statusLabel: 'Скоро' },
  ];

  readonly features: Feature[] = [
    {
      icon: 'pi-calendar',
      title: 'Планирование мероприятий',
      desc: 'Создавайте и настраивайте события с расписанием, датами и описаниями в несколько кликов.',
      delay: 180,
    },
    {
      icon: 'pi-users',
      title: 'Управление участниками',
      desc: 'Отслеживайте регистрации, статусы оплаты и контактную информацию всех участников.',
      delay: 260,
    },
    {
      icon: 'pi-building',
      title: 'Организации',
      desc: 'Объединяйте мероприятия по организациям и разграничивайте доступ для команды.',
      delay: 340,
    },
    {
      icon: 'pi-star',
      title: 'Отзывы и рейтинги',
      desc: '(а у нас есть такое?) Собирайте обратную связь от участников и улучшайте качество каждого события.',
      delay: 420,
    },
    {
      icon: 'pi-chart-bar',
      title: 'Статистика',
      desc: 'Наглядные показатели по участникам, мероприятиям и активности на дашборде.',
      delay: 500,
    },
    {
      icon: 'pi-shield',
      title: 'Безопасность',
      desc: 'Ваши данные хранятся на зашифрованных серверах.',
      delay: 580,
    },
  ];

  readonly stats: Stat[] = [
    { value: '10 000+', label: 'Участников', delay: 0 },
    { value: '500+', label: 'Мероприятий', delay: 80 },
    { value: '120+', label: 'Организаций', delay: 160 },
    { value: '5', label: 'Средний рейтинг', delay: 240 },
  ];
}

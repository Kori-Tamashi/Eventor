import {
  EventDetailsDrawerContext,
  EventDetailsDrawerDay,
  EventDetailsDrawerSource,
  EventDetailsDrawerViewerRole,
} from './event-details-drawer.models';

export function buildMockEventDetailsDrawerContext(
  title: string,
  source: EventDetailsDrawerSource,
  viewerRole: EventDetailsDrawerViewerRole = 'user'
): EventDetailsDrawerContext {
  const dayRows: EventDetailsDrawerDay[] = [
    {
      id: 'day-1',
      name: 'День 1 - Название',
      price: 'N',
      participants: 4,
      participantRows: [
        { name: 'Участник 1', payment: 'Оплачено' },
        { name: 'Участник 2', payment: 'Оплачено' },
        { name: 'Участник 3', payment: 'Не оплачено' },
        { name: 'Участник 4', payment: 'Оплачено' },
      ],
    },
    {
      id: 'day-2',
      name: 'День 2 - Название',
      price: 'N',
      participants: 6,
      participantRows: [
        { name: 'Участник 1', payment: 'Оплачено' },
        { name: 'Участник 2', payment: 'Не оплачено' },
        { name: 'Участник 3', payment: 'Оплачено' },
        { name: 'Участник 4', payment: 'Оплачено' },
        { name: 'Участник 5', payment: 'Не оплачено' },
        { name: 'Участник 6', payment: 'Оплачено' },
      ],
    },
    {
      id: 'day-3',
      name: 'День 3 - Название',
      price: 'N',
      participants: 3,
      participantRows: [
        { name: 'Участник 1', payment: 'Оплачено' },
        { name: 'Участник 2', payment: 'Оплачено' },
        { name: 'Участник 3', payment: 'Не оплачено' },
      ],
    },
  ];

  const reviewRows = [
    { person: 'Александр', comment: 'Хорошая организация, всё прошло по плану.', rating: 5 },
    { person: 'Мария', comment: 'Понравилась программа и общее сопровождение мероприятия.', rating: 4 },
    { person: 'Иван', comment: 'Удобная логистика и понятное расписание.', rating: 5 },
  ];

  const participantsCount = dayRows.reduce((sum, day) => sum + day.participantRows.length, 0);

  return {
    eventId: 'event-mock',
    source,
    viewerRole,
    data: {
      title,
      daysCount: dayRows.length,
      participantsCount,
      dayRows,
      reviewRows,
      currentUserId: null,
      currentUsername: null,
      currentUserRegistrationId: null,
      currentUserRegistrationType: null,
      currentUserRegistrationPayment: null,
      currentUserRegistrationDayIds: [],
    },
  };
}

export type EventDetailsDrawerSource = 'dashboard' | 'events' | 'organization';
export type EventDetailsDrawerViewerRole = 'user' | 'superuser';

export type EventDetailsDrawerParticipantPayment = 'Оплачено' | 'Не оплачено';

export type EventDetailsDrawerParticipant = {
  name: string;
  payment: EventDetailsDrawerParticipantPayment;
};

export type EventDetailsDrawerDay = {
  name: string;
  price: string;
  participants: number;
  participantRows: EventDetailsDrawerParticipant[];
};

export type EventDetailsDrawerReview = {
  person: string;
  comment: string;
  rating: number;
};

export type EventDetailsDrawerData = {
  title: string;
  daysCount: number;
  participantsCount: number;
  dayRows: EventDetailsDrawerDay[];
  reviewRows: EventDetailsDrawerReview[];
};

export type EventDetailsDrawerContext = {
  source: EventDetailsDrawerSource;
  viewerRole: EventDetailsDrawerViewerRole;
  data: EventDetailsDrawerData;
};

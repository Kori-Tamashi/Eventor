export type EventDetailsDrawerSource = 'dashboard' | 'events' | 'organization';
export type EventDetailsDrawerViewerRole = 'user' | 'superuser';

export type EventDetailsDrawerParticipantPayment = 'Оплачено' | 'Не оплачено';

export type EventDetailsDrawerParticipant = {
  name: string;
  payment: EventDetailsDrawerParticipantPayment;
};

export type EventDetailsDrawerDay = {
  id: string;
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
  currentUserRegistrationId: string | null;
};

export type EventDetailsDrawerContext = {
  eventId: string;
  source: EventDetailsDrawerSource;
  viewerRole: EventDetailsDrawerViewerRole;
  data: EventDetailsDrawerData;
};

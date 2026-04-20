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
  feedbackId: string;
  person: string;
  comment: string;
  rating: number;
  isOwnedByCurrentUser: boolean;
};

export type EventDetailsDrawerRegistrationType = 0 | 1 | 2;

export type EventDetailsDrawerData = {
  title: string;
  daysCount: number;
  participantsCount: number;
  dayRows: EventDetailsDrawerDay[];
  reviewRows: EventDetailsDrawerReview[];
  currentUserId: string | null;
  currentUsername: string | null;
  currentUserRegistrationId: string | null;
  currentUserRegistrationType: EventDetailsDrawerRegistrationType | null;
  currentUserRegistrationPayment: boolean | null;
  currentUserRegistrationDayIds: string[];
};

export type EventDetailsDrawerContext = {
  eventId: string;
  source: EventDetailsDrawerSource;
  viewerRole: EventDetailsDrawerViewerRole;
  data: EventDetailsDrawerData;
};

export type FeedbackApiModel = {
  id: string;
  registrationId: string;
  comment: string;
  rate: number;
};

export type CreateFeedbackPayload = {
  registrationId: string;
  comment: string;
  rate: number;
};

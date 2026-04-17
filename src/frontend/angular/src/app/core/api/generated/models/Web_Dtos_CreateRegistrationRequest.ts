/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Web_Dtos_RegistrationType } from './Web_Dtos_RegistrationType';
export type Web_Dtos_CreateRegistrationRequest = {
    eventId: string;
    userId: string;
    type: Web_Dtos_RegistrationType;
    payment?: boolean;
    dayIds: Array<string>;
};


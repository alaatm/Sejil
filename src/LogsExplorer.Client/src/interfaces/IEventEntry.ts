import IEventEntryProperty from './IEventEntryProperty';

export default interface IEventEntry {
    id: string;
    message: string;
    messageTemplate: string;
    level: string;
    timestamp: string;
    exception: string | null;
    sourceContext: string;
    requestId: string;
    properties: IEventEntryProperty[];
}

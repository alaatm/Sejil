import ILogEntryProperty from './ILogEntryProperty';

export default interface ILogEntry {
    id: string;
    message: string;
    messageTemplate: string;
    level: string;
    timestamp: string;
    exception: string | null;
    sourceContext: string;
    requestId: string;
    properties: ILogEntryProperty[];
}

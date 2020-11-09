export function isEnterKey(e: React.KeyboardEvent<HTMLInputElement>) {
    const code = e.key ?? e.keyCode?.toString();
    return code && (code === 'Enter' || code === '13');
}
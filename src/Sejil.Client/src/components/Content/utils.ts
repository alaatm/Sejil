const monthNames = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
];

export function formatDate(date: string) {
    const d = new Date(date);

    const hours = d.getHours();
    const minutes = d.getMinutes();
    const seconds = d.getSeconds();
    const milliSeconds = d.getMilliseconds();
    const h = hours < 10 ? '0' + hours : hours;
    const m = minutes < 10 ? '0' + minutes : minutes;
    const s = seconds < 10 ? '0' + seconds : seconds;
    const ms = milliSeconds < 10 ? '00' + milliSeconds : milliSeconds < 100 ? '0' + milliSeconds : milliSeconds;
    const time = h + ':' + m + ':' + s + '.' + ms;

    const day = d.getDate();
    return (day < 10 ? '0' + day : day) + ' ' + monthNames[d.getMonth()] + ' ' + d.getFullYear() + ' ' + time;
}

export function levelToColor(level: string) {
    switch (level) {
        case 'Verbose': return '#d3d3d3';
        case 'Debug': return '#9e9e9e';
        case 'Information': return '#007acc';
        case 'Warning': return '#faad14';
        case 'Error': return '#ff4d4f';
        case 'Critical': return '#d81b60';
    }
}

export const levelMap: { [key: string]: string } = {
    "Trace": "verbose",
    "Verbose": "verbose",
    "Debug": "debug",
    "Information": "information",
    "Warning": "warning",
    "Error": "error",
    "Critical": "critical",
    "Fatal": "critical",
};

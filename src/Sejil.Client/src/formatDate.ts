// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

export function formatLogEntryDate(date: Date | string) {
    const d = date instanceof Date
        ? date
        : new Date(date);

    const hours = d.getHours();
    const minutes = d.getMinutes();
    const seconds = d.getSeconds();
    const milliSeconds = d.getMilliseconds();
    const h = hours < 10 ? '0' + hours : hours;
    const m = minutes < 10 ? '0' + minutes : minutes;
    const s = seconds < 10 ? '0' + seconds : seconds;
    const ms = milliSeconds < 10 ? '00' + milliSeconds : milliSeconds < 100 ? '0' + milliSeconds : milliSeconds;
    const time = h + ':' + m + ':' + s + '.' + ms;

    return pad(d.getDate(), 2) + ' ' + getMonthName(d.getMonth()) + ' ' + d.getFullYear() + ' ' + time;
}

export function formatServerDate(date: Date) {
    return `${date.getFullYear()}-${pad(date.getMonth() + 1, 2)}-${pad(date.getDate(), 2)}`;
}

function getMonthName(n: number) {
    switch (n) {
        case 0: return 'Jan';
        case 1: return 'Feb';
        case 2: return 'Mar';
        case 3: return 'Apr';
        case 4: return 'May';
        case 5: return 'Jun';
        case 6: return 'Jul';
        case 7: return 'Aug';
        case 8: return 'Sep';
        case 9: return 'Oct';
        case 10: return 'Nov';
        case 11: return 'Dec';
    }

    throw new Error('Invalid month number.');
}

function pad(num: number, size: number) {
    const s = '000000000' + num;
    return s.substr(s.length - size);
}
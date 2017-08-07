// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

import formatDate from '../src/formatDate';

describe('dateFormat() tests', () => {
    it('should return a date formatted as dd MMM yyyy HH:mm:ss.fff', async () => {
        expect(formatDate(new Date(2017, 1, 1, 1, 1, 1, 1)))
            .toBe('01 Feb 2017 01:01:01.001');
        expect(formatDate(new Date(2017, 10, 10, 10, 10, 10, 10)))
            .toBe('10 Nov 2017 10:10:10.010');
        expect(formatDate(new Date(2017, 10, 10, 10, 10, 10, 100)))
            .toBe('10 Nov 2017 10:10:10.100');
        expect(formatDate('Fri Nov 10 2017 10:10:10.100'))
            .toBe('10 Nov 2017 10:10:10.100');

        for (let i = 0; i < 12; i++) {
            expect(formatDate(new Date(2017, i, 1, 1, 1, 1, 1)))
                .toBe(`01 ${getMonth(i)} 2017 01:01:01.001`);
        }
    });
});

function getMonth(i: number) {
    switch (i) {
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

    return '';
}
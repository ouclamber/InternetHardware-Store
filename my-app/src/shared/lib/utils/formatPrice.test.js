import { formatPrice, formatDate } from './formatPrice';

// Нормализуем пробелы — заменяем неразрывные (\u00A0) на обычные
const normalize = (str) => str.replace(/\u00A0/g, ' ');

describe('formatPrice', () => {
    test('форматирует цену с разделителями тысяч', () => {
        expect(normalize(formatPrice(1000))).toBe('1 000 ₽');
        expect(normalize(formatPrice(54999))).toBe('54 999 ₽');
        expect(normalize(formatPrice(1000000))).toBe('1 000 000 ₽');
    });

    test('обрабатывает 0', () => {
        expect(normalize(formatPrice(0))).toBe('0 ₽');
    });

    test('возвращает "0 ₽" для undefined', () => {
        expect(normalize(formatPrice(undefined))).toBe('0 ₽');
    });

    test('возвращает "0 ₽" для null', () => {
        expect(normalize(formatPrice(null))).toBe('0 ₽');
    });

    test('возвращает "0 ₽" для NaN', () => {
        expect(normalize(formatPrice(NaN))).toBe('0 ₽');
    });

    test('обрабатывает отрицательные числа', () => {
        expect(normalize(formatPrice(-500))).toBe('-500 ₽');
    });
});

describe('formatDate', () => {
    test('форматирует корректную дату в ru-RU', () => {
        const result = formatDate('2024-03-15T10:00:00');
        expect(result).toMatch(/\d{2}\.\d{2}\.\d{4}/);
    });

    test('возвращает "Дата не указана" для undefined', () => {
        expect(formatDate(undefined)).toBe('Дата не указана');
    });

    test('возвращает "Дата не указана" для null', () => {
        expect(formatDate(null)).toBe('Дата не указана');
    });

    test('возвращает "Дата не указана" для невалидной строки', () => {
        expect(formatDate('not-a-date')).toBe('Дата не указана');
    });
});
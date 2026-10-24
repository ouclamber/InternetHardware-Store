import { detectProductType } from './detectProductType';

const RULES = [
    [['jbl charge'], 'jbl-charge'],
    [['jbl'], 'jbl'],
    [['oled', 'qled'], 'premium']
];

describe('detectProductType', () => {
    test('первое подходящее правило побеждает', () => {
        expect(detectProductType('JBL Charge 5', RULES)).toBe('jbl-charge');
        expect(detectProductType('JBL Flip 6', RULES)).toBe('jbl');
    });

    test('любое из ключевых слов правила', () => {
        expect(detectProductType('Samsung QLED 55', RULES)).toBe('premium');
    });

    test('без совпадений — значение по умолчанию', () => {
        expect(detectProductType('Утюг', RULES)).toBe('standard');
        expect(detectProductType('Утюг', RULES, 'other')).toBe('other');
    });

    test('пустое имя', () => {
        expect(detectProductType('', RULES)).toBe('standard');
        expect(detectProductType(null, RULES)).toBe('standard');
    });

    test('без правил', () => {
        expect(detectProductType('JBL')).toBe('standard');
    });
});

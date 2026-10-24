/**
 * Определяет CSS-«тип» товара по названию на основе списка правил.
 * Правило: [ключевые слова, тип]; срабатывает первое, где встретилось любое слово.
 *
 *   detectProductType('Apple iPhone 15', [[['iphone', 'apple'], 'iphone']]) → 'iphone'
 */
export const detectProductType = (name, rules = [], fallback = 'standard') => {
    if (!name) return fallback;
    const lower = String(name).toLowerCase();

    for (const [keywords, type] of rules) {
        if (keywords.some(keyword => lower.includes(keyword))) return type;
    }
    return fallback;
};

export const formatPrice = (price) => {
    if (price === undefined || price === null || isNaN(price)) return '0 ₽';
    return `${Number(price).toLocaleString('ru-RU')} ₽`;
};

export const formatDate = (dateString) => {
    if (!dateString) return 'Дата не указана';
    try {
        const date = new Date(dateString);
        if (isNaN(date.getTime())) return 'Дата не указана';
        return date.toLocaleDateString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    } catch {
        return 'Дата не указана';
    }
};
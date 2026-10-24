import React from 'react';
import './Speaker.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import CategoryCatalog from 'widgets/category-catalog/ui/CategoryCatalog';

// Подсказки бренда по названию (если бэкенд не вернул BrandName)
export const SPEAKER_BRANDS = {
    'jbl': 'JBL', 'sony': 'Sony',
    'sennheiser': 'Sennheiser', 'xiaomi': 'Xiaomi'
};

// CSS-тип карточки по ключевым словам в названии
export const SPEAKER_TYPES = [
    [['jbl charge'], 'jbl-charge'],
    [['jbl flip'], 'jbl-flip'],
    [['sony'], 'sony'],
    [['sennheiser'], 'sennheiser'],
    [['xiaomi'], 'xiaomi']
];

const Speaker = ({ navigate }) => (
    <CategoryCatalog
        navigate={navigate}
        title="Колонки"
        categoryName="Колонки"
        brandHints={SPEAKER_BRANDS}
        typeRules={SPEAKER_TYPES}
        placeholderText="Колонка"
        pageClassName="speakers-page"
        containerClassName="speakers-container"
    />
);

export default withNavigate(Speaker);

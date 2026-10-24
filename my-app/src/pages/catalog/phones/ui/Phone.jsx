import React from 'react';
import './Phone.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import CategoryCatalog from 'widgets/category-catalog/ui/CategoryCatalog';

// Подсказки бренда по названию (если бэкенд не вернул BrandName)
export const PHONE_BRANDS = {
    'iphone': 'Apple', 'apple': 'Apple',
    'galaxy': 'Samsung', 'samsung': 'Samsung',
    'xiaomi': 'Xiaomi', 'redmi': 'Xiaomi',
    'pixel': 'Google', 'google': 'Google',
    'xperia': 'Sony', 'sony': 'Sony'
};

// CSS-тип карточки по ключевым словам в названии
export const PHONE_TYPES = [
    [['iphone', 'apple'], 'iphone'],
    [['galaxy', 'samsung'], 'samsung'],
    [['xiaomi', 'redmi'], 'xiaomi'],
    [['pixel', 'google'], 'google'],
    [['xperia', 'sony'], 'sony']
];

const Phone = ({ navigate }) => (
    <CategoryCatalog
        navigate={navigate}
        title="Смартфоны"
        categoryName="Смартфоны"
        brandHints={PHONE_BRANDS}
        typeRules={PHONE_TYPES}
        placeholderText="Смартфон"
        pageClassName="smartphones-page"
        containerClassName="smartphones-container"
    />
);

export default withNavigate(Phone);

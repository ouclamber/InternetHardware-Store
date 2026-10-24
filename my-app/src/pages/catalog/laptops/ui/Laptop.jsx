import React from 'react';
import './Laptop.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import CategoryCatalog from 'widgets/category-catalog/ui/CategoryCatalog';

// Подсказки бренда по названию (если бэкенд не вернул BrandName)
export const LAPTOP_BRANDS = {
    'apple': 'Apple', 'macbook': 'Apple',
    'asus': 'ASUS', 'lenovo': 'Lenovo',
    'xiaomi': 'Xiaomi', 'redmibook': 'Xiaomi'
};

// CSS-тип карточки по ключевым словам в названии
export const LAPTOP_TYPES = [
    [['macbook', 'apple'], 'macbook'],
    [['gaming', 'tuf'], 'gaming'],
    [['vivobook'], 'vivobook'],
    [['ideapad'], 'ideapad'],
    [['redmibook'], 'xiaomi']
];

const Laptop = ({ navigate }) => (
    <CategoryCatalog
        navigate={navigate}
        title="Ноутбуки"
        categoryName="Ноутбуки"
        brandHints={LAPTOP_BRANDS}
        typeRules={LAPTOP_TYPES}
        placeholderText="Ноутбук"
        pageClassName="laptop-page"
        containerClassName="laptop-container"
    />
);

export default withNavigate(Laptop);

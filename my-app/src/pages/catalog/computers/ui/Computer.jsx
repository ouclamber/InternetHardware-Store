import React from 'react';
import './Computer.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import CategoryCatalog from 'widgets/category-catalog/ui/CategoryCatalog';

// Подсказки бренда по названию (если бэкенд не вернул BrandName)
export const COMPUTER_BRANDS = {
    'msi': 'MSI', 'hp': 'HP', 'dell': 'Dell', 'asus': 'ASUS', 'lenovo': 'Lenovo'
};

// CSS-тип карточки по ключевым словам в названии
export const COMPUTER_TYPES = [
    [['gaming', 'игровой'], 'gaming'],
    [['office', 'офисный'], 'office'],
    [['home', 'домашний'], 'home'],
    [['budget', 'бюджетный'], 'budget']
];

const Computer = ({ navigate }) => (
    <CategoryCatalog
        navigate={navigate}
        title="Компьютеры"
        categoryName="Компьютеры"
        brandHints={COMPUTER_BRANDS}
        typeRules={COMPUTER_TYPES}
        placeholderText="Компьютер"
        pageClassName="computer-page"
        containerClassName="computer-container"
    />
);

export default withNavigate(Computer);

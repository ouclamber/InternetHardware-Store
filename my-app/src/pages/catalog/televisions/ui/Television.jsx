import React from 'react';
import './Television.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import CategoryCatalog from 'widgets/category-catalog/ui/CategoryCatalog';

// Подсказки бренда по названию (если бэкенд не вернул BrandName)
export const TELEVISION_BRANDS = {
    'samsung': 'Samsung', 'lg': 'LG',
    'sony': 'Sony', 'xiaomi': 'Xiaomi'
};

// CSS-тип карточки по ключевым словам в названии
export const TELEVISION_TYPES = [
    [['samsung'], 'samsung'],
    [['lg'], 'lg'],
    [['sony'], 'sony'],
    [['xiaomi'], 'xiaomi'],
    [['oled'], 'oled'],
    [['qled'], 'qled']
];

const Television = ({ navigate }) => (
    <CategoryCatalog
        navigate={navigate}
        title="Телевизоры"
        categoryName="Телевизоры"
        brandHints={TELEVISION_BRANDS}
        typeRules={TELEVISION_TYPES}
        placeholderText="Телевизор"
        pageClassName="television-page"
        containerClassName="television-container"
    />
);

export default withNavigate(Television);

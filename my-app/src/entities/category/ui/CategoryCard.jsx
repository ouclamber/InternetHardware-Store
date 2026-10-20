import React from 'react';
import './CategoryCard.css';

const CategoryCard = ({ category, index, onClick, getCategoryImage }) => {
    const colorClass = `category-color-${(index % 5) + 1}`;
    const isEmpty = category.productCount === 0;
    const imageUrl = getCategoryImage(category);

    return (
        <div 
            className={`category-card ${colorClass} ${isEmpty ? 'empty' : ''}`}
            onClick={() => onClick(category.id, category.name)}
        >
            <div className="category-image-container">
                {imageUrl ? (
                    <img 
                        src={imageUrl} 
                        alt={category.name}
                        className="category-image"
                        loading="lazy"
                        onError={(e) => {
                            e.target.onerror = null;
                            e.target.style.display = 'none';
                            const parent = e.target.parentElement;
                            if (parent) {
                                const noImageDiv = document.createElement('div');
                                noImageDiv.className = 'category-no-image';
                                noImageDiv.textContent = '';
                                noImageDiv.style.cssText = 'display: flex; align-items: center; justify-content: center; width: 100%; height: 100%; font-size: 48px; background: #f5f5f5;';
                                parent.appendChild(noImageDiv);
                            }
                        }}
                    />
                ) : (
                    <div className="category-no-image"></div>
                )}
                <div className="category-overlay"></div>
            </div>
            <div className="category-content">
                <div className="category-header">
                    <h3 className="category-name">{category.name}</h3>
                    <span className="category-badge">
                        {category.productCount} товар{category.productCount !== 1 ? 'ов' : ''}
                    </span>
                </div>
                {category.description && (
                    <p className="category-description">
                        {category.description.length > 80 
                            ? `${category.description.substring(0, 80)}...` 
                            : category.description}
                    </p>
                )}
                <div className="category-footer">
                    <span className="category-action">
                        Смотреть товары
                        <svg className="arrow-icon" width="16" height="16" viewBox="0 0 24 24" fill="none">
                            <path d="M5 12H19" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                            <path d="M12 5L19 12L12 19" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                        </svg>
                    </span>
                </div>
            </div>
        </div>
    );
};

export default CategoryCard;
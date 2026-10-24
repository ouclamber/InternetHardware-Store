import React from 'react';
import './CategoryCard.css';

// Лёгкий 3D-наклон и «блик» за курсором — только через CSS-переменные, без ре-рендеров.
// Обновление не чаще одного раза за кадр: mousemove приходит чаще, чем браузер рисует.
const pendingFrames = new WeakMap();

const handlePointerMove = (e) => {
    const el = e.currentTarget;
    const { clientX, clientY } = e;
    if (pendingFrames.has(el)) {
        pendingFrames.get(el).x = clientX;
        pendingFrames.get(el).y = clientY;
        return;
    }

    const point = { x: clientX, y: clientY };
    pendingFrames.set(el, point);

    const schedule = window.requestAnimationFrame || ((cb) => setTimeout(cb, 16));
    schedule(() => {
        // курсор ушёл с карточки до отрисовки кадра — наклон не применяем
        if (pendingFrames.get(el) !== point) return;
        pendingFrames.delete(el);
        const rect = el.getBoundingClientRect();
        if (!rect.width || !rect.height) return;
        const x = (point.x - rect.left) / rect.width;
        const y = (point.y - rect.top) / rect.height;
        el.style.setProperty('--mx', `${(x * 100).toFixed(1)}%`);
        el.style.setProperty('--my', `${(y * 100).toFixed(1)}%`);
        el.style.setProperty('--rx', `${((0.5 - y) * 6).toFixed(2)}deg`);
        el.style.setProperty('--ry', `${((x - 0.5) * 8).toFixed(2)}deg`);
    });
};

const handlePointerLeave = (e) => {
    const el = e.currentTarget;
    pendingFrames.delete(el);
    el.style.setProperty('--rx', '0deg');
    el.style.setProperty('--ry', '0deg');
};

const CategoryCard = ({ category, index, onClick, getCategoryImage }) => {
    const colorClass = `category-color-${(index % 5) + 1}`;
    const isEmpty = category.productCount === 0;
    const imageUrl = getCategoryImage(category);

    return (
        <div 
            className={`category-card ${colorClass} ${isEmpty ? 'empty' : ''}`}
            onClick={() => onClick(category.id, category.name)}
            onMouseMove={handlePointerMove}
            onMouseLeave={handlePointerLeave}
            style={{ '--i': index }}
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
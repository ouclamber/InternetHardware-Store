import React from 'react';
import './CatalogLayout.css';

const CatalogLayout = ({
    title,
    filters,
    children,
    pagination
}) => {
    return (
        <div className="catalog-layout">
            <div className="catalog-layout-header">
                <h1 className="page-title">{title}</h1>
            </div>

            <div className="catalog-layout-body">
                <aside className="search-filters">
                    {filters}
                </aside>

                <div className="catalog-layout-results">
                    {children}
                    {pagination}
                </div>
            </div>
        </div>
    );
};

export default CatalogLayout;
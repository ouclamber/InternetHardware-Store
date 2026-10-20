import { useNavigate, useLocation } from 'react-router-dom';
import React from 'react';

export function withNavigate(Component) {
    function WrappedComponent(props) {
        const navigate = useNavigate();
        const location = useLocation();
        return <Component {...props} navigate={navigate} location={location} />;
    }
    return WrappedComponent;
}
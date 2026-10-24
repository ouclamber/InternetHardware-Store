import React from 'react';
import { render } from '@testing-library/react';
import { withNavigate } from './withNavigate';

const mockNavigate = jest.fn();
const mockLocation = { pathname: '/test', search: '', hash: '' };

jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
    useLocation: () => mockLocation
}));

describe('withNavigate', () => {
    test('пробрасывает navigate в обёрнутый компонент', () => {
        const TestComponent = ({ navigate }) => (
            <button onClick={() => navigate('/test')}>Click</button>
        );

        const Wrapped = withNavigate(TestComponent);
        const { getByText } = render(<Wrapped />);

        getByText('Click').click();

        expect(mockNavigate).toHaveBeenCalledWith('/test');
    });

    test('пробрасывает location в обёрнутый компонент', () => {
        const TestComponent = ({ location }) => (
            <div data-testid="path">{location.pathname}</div>
        );

        const Wrapped = withNavigate(TestComponent);
        const { getByTestId } = render(<Wrapped />);

        expect(getByTestId('path')).toHaveTextContent('/test');
    });

    test('пробрасывает остальные props', () => {
        const TestComponent = ({ text }) => <div>{text}</div>;
        const Wrapped = withNavigate(TestComponent);
        const { getByText } = render(<Wrapped text="Hello" />);
        expect(getByText('Hello')).toBeInTheDocument();
    });
});
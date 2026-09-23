import { useNavigate } from 'react-router';
import './NotFound.css';

export function NotFound() {
  const navigate = useNavigate();
  const token = localStorage.getItem('authToken');

  const handleGoHome = () => {
    // If they are logged in, send them to dashboard; otherwise, send them to login
    if (token) {
      navigate('/dashboard');
    } else {
      navigate('/login');
    }
  };

  return (
    <div className="not-found-container">
      <div className="not-found-content">
        <span className="not-found-icon">🗺️</span>
        <h1>404 - Page Off Course</h1>
        <p>Looks like you took a wrong turn. This route doesn't exist on our map.</p>
        <button className="btn-primary" onClick={handleGoHome}>
          Back to Safe Tracks
        </button>
      </div>
    </div>
  );
}

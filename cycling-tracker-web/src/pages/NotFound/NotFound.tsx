import { useNavigate } from 'react-router';
import './NotFound.css';

export function NotFound() {
  const navigate = useNavigate();

  return (
    <div className="not-found-container">
      <div className="not-found-content">
        <span className="not-found-icon">🗺️</span>
        <h1>404 - Page Off Course</h1>
        <p>Looks like you took a wrong turn. This route doesn't exist on our map.</p>
        <button className="btn btn-primary" onClick={() => navigate('/dashboard')}>
          Back to Safe Tracks
        </button>
      </div>
    </div>
  );
}

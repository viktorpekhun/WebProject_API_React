import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import Button from 'react-bootstrap/Button';
import useAuth from "@/hooks/useAuth.jsx";
import {useNavigate, Link} from "react-router-dom";
import useLogout from "@/hooks/useLogout.jsx";

function BasicExample() {
    const {auth} = useAuth();
    const navigate = useNavigate();
    const logout = useLogout();


    const signOut = async () => {
        await logout();
        navigate('/welcomepage');
    }


    return (
        <Navbar expand="lg" className="bg-body-tertiary">
            <Container>
                <Navbar.Brand as={Link} to="/welcomepage">WebProject</Navbar.Brand>
                <Navbar.Toggle aria-controls="basic-navbar-nav" />
                <Navbar.Collapse id="basic-navbar-nav">
                    {auth?.username ? (
                        <Nav className="ms-auto">
                            <Container className="d-flex align-items-center justify-content-center">
                                <Navbar.Text>Welcome, {auth.username}</Navbar.Text>
                            </Container>
                            <Button variant="secondary" onClick={signOut} className="m-0">
                                Logout
                            </Button>
                        </Nav>
                    ) : (
                        <Nav className="ms-auto">
                            <Button as={Link} to="/login" variant="primary" style={{ marginRight: '8px'}}>
                                Login
                            </Button>
                            <Button as={Link} to="/register" variant="secondary">
                                Register
                            </Button>
                        </Nav>
                    )}
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default BasicExample;
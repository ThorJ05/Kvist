import { useEffect, useState, type FormEvent } from 'react';

// Når siden serveres af selve API'et (Docker, port 8080 / 5244) bruges relative stier.
// Under udvikling med Bun (anden port) peges der på API'et på localhost:5244.
const API = ['5244', '8080'].includes(location.port) ? '/api' : 'http://localhost:5244/api';

type Role = 'User' | 'SuperUser' | 'Admin';

interface UserDto {
    id: string;
    username: string;
    roles: string[];
}

interface Me {
    id: string;
    username: string;
    roles: string[];
}

const rank = (roles: string[]) =>
    Math.max(0, ...roles.map(r => (r === 'Admin' ? 3 : r === 'SuperUser' ? 2 : r === 'User' ? 1 : 0)));

async function readError(res: Response): Promise<string> {
    const text = await res.text();
    try {
        const j = JSON.parse(text);
        if (Array.isArray(j)) return j.join(' ');
        if (typeof j === 'string') return j;
        if (j?.errors) return (Object.values(j.errors).flat() as string[]).join(' ');
        if (j?.title) return j.title;
    } catch {
        /* ikke JSON */
    }
    return text || `Error ${res.status}`;
}

export default function App() {
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
    const [me, setMe] = useState<Me | null>(null);
    const [users, setUsers] = useState<UserDto[]>([]);
    const [message, setMessage] = useState('');

    // login
    const [loginName, setLoginName] = useState('');
    const [loginPassword, setLoginPassword] = useState('');

    // opret / rediger
    const [editing, setEditing] = useState<UserDto | null>(null);
    const [formName, setFormName] = useState('');
    const [formPassword, setFormPassword] = useState('');
    const [formRole, setFormRole] = useState<Role>('User');

    const callerRank = me ? rank(me.roles) : 0;
    const canEditUsers = callerRank >= 2;
    const assignableRoles: Role[] = callerRank === 3 ? ['User', 'SuperUser', 'Admin'] : ['User'];

    const canManage = (u: UserDto) => callerRank === 3 || (callerRank === 2 && rank(u.roles) <= 1);

    const request = (path: string, options: RequestInit = {}) =>
        fetch(`${API}${path}`, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
                ...options.headers,
            },
        });

    function logout() {
        localStorage.removeItem('token');
        setToken(null);
        setMe(null);
        setUsers([]);
        setEditing(null);
    }

    async function loadUsers() {
        const res = await request('/users');
        if (res.ok) setUsers(await res.json());
        else if (res.status === 401) logout();
    }

    useEffect(() => {
        if (!token) return;
        request('/auth/me')
            .then(r => (r.ok ? r.json() : Promise.reject()))
            .then((d: Me) => {
                setMe(d);
                return loadUsers();
            })
            .catch(() => logout());
    }, [token]);

    async function handleLogin(e: FormEvent) {
        e.preventDefault();
        setMessage('');
        try {
            const res = await fetch(`${API}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username: loginName, password: loginPassword }),
            });
            if (!res.ok) {
                setMessage(res.status === 423 ? 'Account locked. Try again later.' : 'Invalid username or password.');
                return;
            }
            const data = await res.json();
            localStorage.setItem('token', data.token);
            setToken(data.token);
            setLoginPassword('');
        } catch {
            setMessage('Cannot reach the server. Is the API running?');
        }
    }

    function startEdit(u: UserDto) {
        setEditing(u);
        setFormName(u.username);
        setFormPassword('');
        setFormRole((u.roles[0] as Role) ?? 'User');
        setMessage('');
    }

    function resetForm() {
        setEditing(null);
        setFormName('');
        setFormPassword('');
        setFormRole('User');
    }

    async function handleSave(e: FormEvent) {
        e.preventDefault();
        setMessage('');

        const res = editing
            ? await request(`/users/${editing.id}`, {
                method: 'PUT',
                body: JSON.stringify({
                    username: formName,
                    password: formPassword || null,
                    role: formRole,
                }),
            })
            : await request('/users', {
                method: 'POST',
                body: JSON.stringify({ username: formName, password: formPassword, role: formRole }),
            });

        if (!res.ok) {
            setMessage(await readError(res));
            return;
        }
        setMessage(editing ? 'User updated.' : 'User created.');
        resetForm();
        await loadUsers();
    }

    async function handleDelete(u: UserDto) {
        if (!confirm(`Delete ${u.username}?`)) return;
        const res = await request(`/users/${u.id}`, { method: 'DELETE' });
        if (!res.ok) {
            setMessage(await readError(res));
            return;
        }
        setMessage('User deleted.');
        await loadUsers();
    }

    // ---------- Login-visning ----------
    if (!token || !me) {
        return (
            <div style={{ maxWidth: 300, margin: '50px auto', fontFamily: 'sans-serif' }}>
                <h2>Login</h2>
                <form onSubmit={handleLogin}>
                    <input
                        placeholder="Username"
                        value={loginName}
                        required
                        onChange={e => setLoginName(e.target.value)}
                        style={{ width: '100%', padding: 8, marginBottom: 10, boxSizing: 'border-box' }}
                    />
                    <input
                        type="password"
                        placeholder="Password"
                        value={loginPassword}
                        required
                        onChange={e => setLoginPassword(e.target.value)}
                        style={{ width: '100%', padding: 8, marginBottom: 10, boxSizing: 'border-box' }}
                    />
                    <button type="submit" style={{ width: '100%', padding: 10 }}>
                        Login
                    </button>
                </form>
                {message && <p style={{ fontWeight: 'bold' }}>{message}</p>}
            </div>
        );
    }

    // ---------- Brugertabel ----------
    return (
        <div style={{ maxWidth: 700, margin: '40px auto', fontFamily: 'sans-serif', padding: '0 16px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h2>
                    Welcome, {me.username} <small>({me.roles.join(', ')})</small>
                </h2>
                <button onClick={logout}>Log out</button>
            </div>

            <h3>Users</h3>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                <tr style={{ textAlign: 'left', borderBottom: '1px solid #888' }}>
                    <th style={{ padding: 8 }}>Username</th>
                    <th style={{ padding: 8 }}>Role</th>
                    {canEditUsers && <th style={{ padding: 8 }}>Actions</th>}
                </tr>
                </thead>
                <tbody>
                {users.map(u => (
                    <tr key={u.id} style={{ borderBottom: '1px solid #444' }}>
                        <td style={{ padding: 8 }}>{u.username}</td>
                        <td style={{ padding: 8 }}>{u.roles.join(', ')}</td>
                        {canEditUsers && (
                            <td style={{ padding: 8 }}>
                                {canManage(u) && (
                                    <>
                                        <button onClick={() => startEdit(u)}>Edit</button>{' '}
                                        {u.id !== me.id && <button onClick={() => handleDelete(u)}>Delete</button>}
                                    </>
                                )}
                            </td>
                        )}
                    </tr>
                ))}
                </tbody>
            </table>

            {canEditUsers && (
                <>
                    <h3>{editing ? `Edit ${editing.username}` : 'Create user'}</h3>
                    <form onSubmit={handleSave} style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                        <input
                            placeholder="Username"
                            value={formName}
                            required
                            onChange={e => setFormName(e.target.value)}
                            style={{ padding: 8 }}
                        />
                        <input
                            type="password"
                            placeholder={editing ? 'New password (optional)' : 'Password'}
                            value={formPassword}
                            required={!editing}
                            onChange={e => setFormPassword(e.target.value)}
                            style={{ padding: 8 }}
                        />
                        <select
                            value={formRole}
                            onChange={e => setFormRole(e.target.value as Role)}
                            disabled={editing?.id === me.id}
                            style={{ padding: 8 }}
                        >
                            {assignableRoles.map(r => (
                                <option key={r} value={r}>
                                    {r}
                                </option>
                            ))}
                        </select>
                        <button type="submit">{editing ? 'Save' : 'Create'}</button>
                        {editing && (
                            <button type="button" onClick={resetForm}>
                                Cancel
                            </button>
                        )}
                    </form>
                </>
            )}

            {message && <p style={{ fontWeight: 'bold' }}>{message}</p>}
        </div>
    );
}

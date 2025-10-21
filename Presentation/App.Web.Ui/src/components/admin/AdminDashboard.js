import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import Modal from 'react-modal';
import AdminSidebar from './AdminSidebar';
import AdminTopBar from './AdminTopBar';
import AdminContent from './AdminContent';

// Modal content component
const ModalContent = ({ title, onClose, children }) => (
  <div>
    <div style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      flexWrap: 'wrap',
      marginBottom: '1.5rem',
      paddingBottom: '1rem',
      borderBottom: '1px solid #e9ecef'
    }}>
      <h3 style={{ margin: 0, color: '#243483' }}>{title}</h3>
      <button 
        onClick={onClose}
        style={{
          background: 'transparent',
          border: 'none',
          fontSize: '1.5rem',
          cursor: 'pointer',
          color: '#6c757d',
          ':hover': {
            color: '#dc3545'
          }
        }}
      >
        <i className="fas fa-times"></i>
      </button>
    </div>
    {children}
  </div>
);

// Custom modal styles
const customModalStyles = {
  content: {
    top: '50%',
    left: '50%',
    right: 'auto',
    bottom: 'auto',
    marginRight: '-50%',
    transform: 'translate(-50%, -50%)',
    width: window.innerWidth <860? '95%' : '60%',
    maxWidth: window.innerWidth <860? '400px' : '800px',
    borderRadius: '12px',
    boxShadow: '0 10px 40px rgba(0,0,0,0.3)',
    padding: window.innerWidth <860? '1.5rem' : '2rem',
    border: 'none',
    maxHeight: '90vh',
    overflowY: 'auto'
  },
  overlay: {
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    zIndex: 1000
  }
};

const AdminDashboard = ({ user }) => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('users');
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);
  const [isSidebarOpen, setIsSidebarOpen] = useState(!isMobile);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalContent, setModalContent] = useState({ title: '', content: null });
  
  // Table data and loading states
  const [tableData, setTableData] = useState({
    users: [],
    roles: [],
    institutions: [],
    settings: [],
    reports: []
  });
  
  const [isLoading, setIsLoading] = useState({
    users: false,
    roles: false,
    institutions: false,
    settings: false,
    reports: false
  });

  // Table column definitions
  const tableColumns = {
    users: [
      { header: 'ID', accessor: 'id' },
      { header: 'Username', accessor: 'username' },
      { header: 'Email', accessor: 'email' },
      { header: 'Role', accessor: 'role' },
      { header: 'Status', accessor: 'status' },
      { header: 'Last Login', accessor: 'lastLogin' }
    ],
    roles: [
      { header: 'ID', accessor: 'id' },
      { header: 'Role Name', accessor: 'name' },
      { header: 'Description', accessor: 'description' },
      { header: 'Users', accessor: 'userCount' },
      { header: 'Permissions', accessor: 'permissions' }
    ],
    institutions: [
      { header: 'ID', accessor: 'id' },
      { header: 'Name', accessor: 'name' },
      { header: 'Type', accessor: 'type' },
      { header: 'License', accessor: 'license' },
      { header: 'Status', accessor: 'status' },
      { header: 'Created', accessor: 'created' }
    ],
    settings: [
      { header: 'ID', accessor: 'id' },
      { header: 'Setting', accessor: 'name' },
      { header: 'Value', accessor: 'value' },
      { header: 'Category', accessor: 'category' },
      { header: 'Last Modified', accessor: 'lastModified' }
    ],
    reports: [
      { header: 'ID', accessor: 'id' },
      { header: 'Report Name', accessor: 'name' },
      { header: 'Type', accessor: 'type' },
      { header: 'Generated On', accessor: 'generatedOn' },
      { header: 'Status', accessor: 'status' }
    ]
  };

  // Mock data for demonstration
  const mockData = {
    users: [
      { id: 1, username: 'admin', email: 'admin@example.com', role: 'Super Admin', status: 'Active', lastLogin: '2025-10-05 14:30:22' },
      { id: 2, username: 'manager1', email: 'manager@example.com', role: 'Manager', status: 'Active', lastLogin: '2025-10-05 10:15:45' },
      { id: 3, username: 'user1', email: 'user1@example.com', role: 'User', status: 'Inactive', lastLogin: '2025-10-04 09:22:10' }
    ],
    roles: [
      { id: 1, name: 'Super Admin', description: 'Full system access', userCount: 1, permissions: 'All' },
      { id: 2, name: 'Manager', description: 'Can manage users and content', userCount: 5, permissions: 'Manage Users, View Reports' },
      { id: 3, name: 'User', description: 'Basic user access', userCount: 25, permissions: 'View Only' }
    ],
    institutions: [
      { id: 1, name: 'First National Bank', type: 'Bank', license: 'BNK-001', status: 'Active', created: '2025-01-15' },
      { id: 2, name: 'City Credit Union', type: 'Credit Union', license: 'CU-002', status: 'Pending', created: '2025-02-20' },
      { id: 3, name: 'Investment Corp', type: 'Investment', license: 'INV-003', status: 'Active', created: '2025-03-10' }
    ],
    settings: [
      { id: 1, name: 'Max Login Attempts', value: '5', category: 'Security', lastModified: '2025-10-01' },
      { id: 2, name: 'Session Timeout', value: '30 minutes', category: 'Security', lastModified: '2025-09-28' },
      { id: 3, name: 'Email Notifications', value: 'Enabled', category: 'System', lastModified: '2025-09-25' }
    ],
    reports: [
      { id: 1, name: 'User Activity Report', type: 'Activity', generatedOn: '2025-10-05', status: 'Completed' },
      { id: 2, name: 'System Performance', type: 'Performance', generatedOn: '2025-10-04', status: 'Completed' },
      { id: 3, name: 'Security Audit', type: 'Security', generatedOn: '2025-10-03', status: 'In Progress' }
    ]
  };

  // Load tab data (mock implementation)
  const loadTabData = async (tab) => {
    setIsLoading(prev => ({ ...prev, [tab]: true }));
    
    // Simulate API call delay
    setTimeout(() => {
      setTableData(prev => ({ ...prev, [tab]: mockData[tab] || [] }));
      setIsLoading(prev => ({ ...prev, [tab]: false }));
    }, 1000);
  };

  // Handle tab change
  const handleTabChange = (tab) => {
    setActiveTab(tab);
    if (!tableData[tab] || tableData[tab].length === 0) {
      loadTabData(tab);
    }
  };

  // Modal functions
  const openModal = (title, content) => {
    setModalContent({ title, content });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setModalContent({ title: '', content: null });
  };

  // Handle add new item
  const handleAddNew = (tab) => {
    const content = (
      <div>
        <p style={{ marginTop: 0 }}>Add new {tab.slice(0, -1)} form will appear here.</p>
        <div style={{ marginTop: '1.5rem', textAlign: 'right' }}>
          <button
            onClick={closeModal}
            style={{
              background: '#6c757d',
              color: 'white',
              border: 'none',
              padding: '0.5rem 1.25rem',
              borderRadius: '5px',
              cursor: 'pointer',
              marginRight: '0.75rem',
              transition: 'background-color 0.2s ease'
            }}
          >
            Cancel
          </button>
          <button
            onClick={() => {
              toast.success(`${tab.slice(0, -1)} added successfully!`);
              closeModal();
            }}
            style={{
              background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
              color: 'white',
              border: 'none',
              padding: '0.5rem 1.25rem',
              borderRadius: '5px',
              cursor: 'pointer',
              transition: 'all 0.2s ease'
            }}
          >
            Save {tab.slice(0, -1)}
          </button>
        </div>
      </div>
    );
    
    openModal(`Add New ${tab.slice(0, -1)}`, content);
  };

  // Handle edit item
  const handleEditItem = (tab, row) => {
    const content = (
      <div>
        <p style={{ marginTop: 0 }}>Edit {tab.slice(0, -1)} form will appear here.</p>
        <pre style={{ background: '#f8f9fa', padding: '0.75rem', borderRadius: '6px', overflowX: 'auto' }}>
{JSON.stringify(row, null, 2)}
        </pre>
        <div style={{ marginTop: '1.5rem', textAlign: 'right' }}>
          <button
            onClick={closeModal}
            style={{
              background: '#6c757d',
              color: 'white',
              border: 'none',
              padding: '0.5rem 1.25rem',
              borderRadius: '5px',
              cursor: 'pointer',
              marginRight: '0.75rem',
              transition: 'background-color 0.2s ease'
            }}
          >
            Cancel
          </button>
          <button
            onClick={() => {
              toast.success(`${tab.slice(0, -1)} updated successfully!`);
              closeModal();
            }}
            style={{
              background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
              color: 'white',
              border: 'none',
              padding: '0.5rem 1.25rem',
              borderRadius: '5px',
              cursor: 'pointer',
              transition: 'all 0.2s ease'
            }}
          >
            Save changes
          </button>
        </div>
      </div>
    );
    openModal(`Edit ${tab.slice(0, -1)}`, content);
  };

  // Handle delete item
  const handleDeleteItem = (tab, row) => {
    const confirmed = window.confirm(`Delete this ${tab.slice(0, -1)} (ID: ${row.id || row.name || ''})?`);
    if (!confirmed) return;
    // Optimistic local delete for now
    setTableData(prev => ({
      ...prev,
      [tab]: (prev[tab] || []).filter((r) => r !== row)
    }));
    toast.success(`${tab.slice(0, -1)} deleted`);
  };

  useEffect(() => {
    // Handle responsive sidebar
    const handleResize = () => {
      const mobile = window.innerWidth < 768;
      setIsMobile(mobile);
      if (!mobile) {
        setIsSidebarOpen(true);
      } else {
        setIsSidebarOpen(false);
      }
    };
    handleResize();
    window.addEventListener('resize', handleResize);

    // Load initial tab data
    loadTabData('users');

    return () => window.removeEventListener('resize', handleResize);
  }, []);

  return (
    <div style={{ minHeight: '100vh', background: '#f5f7fa', position: 'relative' }}>
      {/* Mobile Overlay */}
      {isMobile && isSidebarOpen && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: 'rgba(0, 0, 0, 0.5)',
            zIndex: 998,
            transition: 'opacity 0.3s ease'
          }}
          onClick={() => setIsSidebarOpen(false)}
        />
      )}
      
      <div style={{ display: 'flex', maxWidth: '1400px', margin: '0 auto', position: 'relative' }}>
        {/* Sidebar */}
        <AdminSidebar
          user={user}
          activeTab={activeTab}
          onTabChange={handleTabChange}
          isMobile={isMobile}
          isSidebarOpen={isSidebarOpen}
          onCloseSidebar={() => setIsSidebarOpen(false)}
        />

        {/* Main Area */}
        <main style={{ 
          flex: 1, 
          padding: isMobile ? '1rem' : '1rem 1.5rem 2rem',
          marginLeft: isMobile ? 0 : 0,
          transition: 'margin-left 0.3s ease'
        }}>
          {/* Top Bar */}
          <AdminTopBar
            user={user}
            isMobile={isMobile}
            isSidebarOpen={isSidebarOpen}
            onToggleSidebar={() => setIsSidebarOpen((s) => !s)}
            onLogout={() => {
              localStorage.removeItem('user');
              localStorage.removeItem('authToken');
              navigate('/');
            }}
          />

          {/* Content Card */}
          <AdminContent
            activeTab={activeTab}
            tableColumns={tableColumns}
            tableData={tableData}
            isLoading={isLoading}
            onAddNew={handleAddNew}
            onEdit={handleEditItem}
            onDelete={handleDeleteItem}
            isMobile={isMobile}
          />
        </main>
      </div>

      {/* Modal */}
      <Modal
        isOpen={isModalOpen}
        onRequestClose={closeModal}
        style={customModalStyles}
        contentLabel="Admin Modal"
      >
        <ModalContent title={modalContent.title} onClose={closeModal}>
          {modalContent.content}
        </ModalContent>
      </Modal>

      {/* Responsive tweaks */}
      <style jsx global>{`
        @media (max-width: 900px) {
          .logout-text { display: none; }
          
          /* Ensure tables are scrollable on mobile */
          .table-container {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
          }
          
          /* Improve touch targets */
          button {
            min-height: 44px;
          }
        }
        
        /* Custom scrollbar for sidebar */
        nav::-webkit-scrollbar {
          width: 4px;
        }
        
        nav::-webkit-scrollbar-track {
          background: transparent;
        }
        
        nav::-webkit-scrollbar-thumb {
          background: rgba(0,0,0,0.1);
          border-radius: 2px;
        }
        
        nav::-webkit-scrollbar-thumb:hover {
          background: rgba(0,0,0,0.2);
        }
      `}</style>
    </div>
  );
};

export default AdminDashboard;

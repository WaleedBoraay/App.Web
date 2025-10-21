import React from 'react';

const AdminSidebar = ({ 
  user, 
  activeTab, 
  onTabChange, 
  isMobile, 
  isSidebarOpen, 
  onCloseSidebar 
}) => {
  const [expandedSections, setExpandedSections] = React.useState({ userManagement: true, systemManagement: true });

  const navItems = [
    {
      id: 'userManagement',
      label: 'User Management',
      icon: 'users-cog',
      isSection: true,
      children: [
        { id: 'users', label: 'Users', icon: 'users', parent: 'userManagement' },
        { id: 'user-roles', label: 'User Roles', icon: 'user-shield', parent: 'userManagement' },
        { id: 'user-permissions', label: 'Permission Overrides', icon: 'user-lock', parent: 'userManagement' },
        { id: 'role-permissions', label: 'Role Permissions', icon: 'shield-alt', parent: 'userManagement' }
      ]
    },
    { id: 'roles', label: 'Roles', icon: 'user-shield' },
    {
      id: 'systemManagement',
      label: 'System Management',
      icon: 'cogs',
      isSection: true,
      children: [
        { id: 'permissions', label: 'Permissions', icon: 'key', parent: 'systemManagement' },
        { id: 'languages', label: 'Languages', icon: 'globe', parent: 'systemManagement' },
        { id: 'resources', label: 'Resources', icon: 'file-alt', parent: 'systemManagement' }
      ]
    },
    {
      id: 'bankManagement',
      label: 'Bank Management',
      icon: 'university',
      isSection: true,
      children: [
        { id: 'entity-profile', label: 'Entity Registration', icon: 'building', parent: 'bankManagement' }
      ]
    },
    { id: 'institutions', label: 'Institutions', icon: 'university' },
    { id: 'settings', label: 'Settings', icon: 'cog' },
    { id: 'reports', label: 'Reports', icon: 'chart-bar' },
  ];

  const toggleSection = (sectionId) => {
    setExpandedSections(prev => ({
      ...prev,
      [sectionId]: !prev[sectionId]
    }));
  };

  return (
    <aside
      style={{
        width: isMobile ? '280px' : '260px',
        transform: isMobile 
          ? `translateX(${isSidebarOpen ? '0' : '-100%'})` 
          : 'translateX(0)',
        transition: 'transform 0.3s ease',
        position: isMobile ? 'fixed' : 'sticky',
        top: 0,
        left: 0,
        minHeight: '120vh',
        background: 'linear-gradient(180deg, #ffffff 0%, #f8f9fa 100%)',
        borderRight: '1px solid #e9ecef',
        boxShadow: isMobile 
          ? '2px 0 20px rgba(0,0,0,0.15)' 
          : '0 2px 10px rgba(0,0,0,0.05)',
        zIndex: 999,
        overflow: 'hidden'
      }}
    >
      {/* Sidebar Header */}
      <div style={{ 
        padding: '1.5rem 1.25rem 1rem', 
        borderBottom: '1px solid #e9ecef',
        background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)'
      }}>
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'space-between',
          color: 'white'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <div style={{
              width: '40px',
              height: '40px',
              background: 'rgba(255,255,255,0.2)',
              borderRadius: '10px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <i className="fas fa-shield-alt" style={{ fontSize: '1.2rem' }}></i>
            </div>
            <div>
              <div style={{ fontWeight: 700, fontSize: '1.1rem', color: '#fff' }}>Admin Panel</div>
              <div style={{ fontSize: '0.8rem', opacity: 0.8, color: '#fff' }}>Management Console</div>
            </div>
          </div>
          {isMobile && (
            <button
              onClick={onCloseSidebar}
              style={{
                background: 'rgba(255,255,255,0.2)',
                border: 'none',
                color: 'white',
                width: '32px',
                height: '32px',
                borderRadius: '6px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}
            >
              <i className="fas fa-times"></i>
            </button>
          )}
        </div>
      </div>

      {/* Navigation */}
      <nav style={{ padding: '1rem 0.75rem', flex: 1, overflowY: 'auto' }}>
        {navItems.map((item, index) => (
          <div key={item.id}>
            {item.isSection ? (
              // Section header with expandable children
              <>
                <button
                  onClick={() => toggleSection(item.id)}
                  style={{
                    width: '100%',
                    textAlign: 'left',
                    padding: '1rem 1.25rem',
                    marginBottom: '0.25rem',
                    border: 'none',
                    borderRadius: '12px',
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '1rem',
                    background: 'transparent',
                    color: '#495057',
                    fontSize: '0.95rem',
                    fontWeight: 600,
                    transition: 'all 0.2s ease'
                  }}
                  onMouseEnter={(e) => {
                    e.target.style.background = '#f8f9fa';
                  }}
                  onMouseLeave={(e) => {
                    e.target.style.background = 'transparent';
                  }}
                >
                  <div style={{
                    width: '20px',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}>
                    <i className={`fas fa-${item.icon}`} style={{ fontSize: '1rem' }}></i>
                  </div>
                  <span style={{ flex: 1 }}>{item.label}</span>
                  <i 
                    className={`fas fa-chevron-${expandedSections[item.id] ? 'down' : 'right'}`}
                    style={{ fontSize: '0.8rem', transition: 'transform 0.2s ease' }}
                  />
                </button>
                
                {/* Children items */}
                {expandedSections[item.id] && item.children && (
                  <div style={{ marginLeft: '1rem', marginBottom: '0.5rem' }}>
                    {item.children.map((child) => (
                      <button
                        key={child.id}
                        onClick={() => {
                          onTabChange(child.id);
                          if (isMobile) onCloseSidebar();
                        }}
                        style={{
                          width: '100%',
                          textAlign: 'left',
                          padding: '0.75rem 1rem',
                          marginBottom: '0.25rem',
                          border: 'none',
                          borderRadius: '8px',
                          cursor: 'pointer',
                          display: 'flex',
                          alignItems: 'center',
                          gap: '0.75rem',
                          background: activeTab === child.id 
                            ? 'linear-gradient(135deg, #243483 0%, #1467EA 100%)' 
                            : 'transparent',
                          color: activeTab === child.id ? 'white' : '#6c757d',
                          fontSize: '0.9rem',
                          fontWeight: activeTab === child.id ? 600 : 500,
                          transition: 'all 0.2s ease',
                          transform: activeTab === child.id ? 'translateX(4px)' : 'translateX(0)',
                          boxShadow: activeTab === child.id 
                            ? '0 4px 12px rgba(36, 52, 131, 0.3)' 
                            : 'none'
                        }}
                        onMouseEnter={(e) => {
                          if (activeTab !== child.id) {
                            e.target.style.background = '#f0f0f0';
                            e.target.style.transform = 'translateX(2px)';
                          }
                        }}
                        onMouseLeave={(e) => {
                          if (activeTab !== child.id) {
                            e.target.style.background = 'transparent';
                            e.target.style.transform = 'translateX(0)';
                          }
                        }}
                      >
                        <div style={{
                          width: '16px',
                          height: '16px',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center'
                        }}>
                          <i className={`fas fa-${child.icon}`} style={{ fontSize: '0.9rem' }}></i>
                        </div>
                        <span>{child.label}</span>
                        {activeTab === child.id && (
                          <div style={{
                            marginLeft: 'auto',
                            width: '6px',
                            height: '6px',
                            background: 'white',
                            borderRadius: '50%'
                          }} />
                        )}
                      </button>
                    ))}
                  </div>
                )}
              </>
            ) : (
              // Regular menu item
              <button
                onClick={() => {
                  onTabChange(item.id);
                  if (isMobile) onCloseSidebar();
                }}
                style={{
                  width: '100%',
                  textAlign: 'left',
                  padding: '1rem 1.25rem',
                  marginBottom: '0.5rem',
                  border: 'none',
                  borderRadius: '12px',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                  background: activeTab === item.id 
                    ? 'linear-gradient(135deg, #243483 0%, #1467EA 100%)' 
                    : 'transparent',
                  color: activeTab === item.id ? 'white' : '#495057',
                  fontSize: '0.95rem',
                  fontWeight: activeTab === item.id ? 600 : 500,
                  transition: 'all 0.2s ease',
                  transform: activeTab === item.id ? 'translateX(4px)' : 'translateX(0)',
                  boxShadow: activeTab === item.id 
                    ? '0 4px 12px rgba(36, 52, 131, 0.3)' 
                    : 'none'
                }}
                onMouseEnter={(e) => {
                  if (activeTab !== item.id) {
                    e.target.style.background = 'transparent';
                    e.target.style.transform = 'translateX(2px)';
                  }
                }}
                onMouseLeave={(e) => {
                  if (activeTab !== item.id) {
                    e.target.style.background = 'transparent';
                    e.target.style.transform = 'translateX(0)';
                  }
                }}
              >
                <div style={{
                  width: '20px',
                  height: '20px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  <i className={`fas fa-${item.icon}`} style={{ fontSize: '1rem' }}></i>
                </div>
                <span>{item.label}</span>
                {activeTab === item.id && (
                  <div style={{
                    marginLeft: 'auto',
                    width: '6px',
                    height: '6px',
                    background: 'white',
                    borderRadius: '50%'
                  }} />
                )}
              </button>
            )}
          </div>
        ))}
      </nav>
      
      {/* Sidebar Footer */}
      <div style={{
        padding: '1rem 1.25rem',
        borderTop: '1px solid #e9ecef',
        background: '#f8f9fa'
      }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '0.75rem',
          color: '#6c757d',
          fontSize: '0.85rem'
        }}>
          <div style={{
            width: '32px',
            height: '32px',
            background: 'linear-gradient(135deg, #243483 0%, #1467EA 100%)',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'white',
            fontSize: '0.9rem'
          }}>
            {user.username?.charAt(0).toUpperCase()}
          </div>
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ fontWeight: 600, color: '#495057', fontSize: '0.9rem' }}>
              {user.username}
            </div>
            <div style={{ fontSize: '0.75rem', opacity: 0.8 }}>
              {user.roles?.[0] || 'Admin'}
            </div>
          </div>
        </div>
      </div>
    </aside>
  );
};

export default AdminSidebar;

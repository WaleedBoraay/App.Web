# Deployment Guide for Bank Portal

## Deployment to http://suptech.online/

### Prerequisites
- Node.js installed
- npm or yarn package manager
- Access to suptech.online server

### Build for Production

1. **Install dependencies** (if not already done):
   ```bash
   npm install
   ```

2. **Build the project for production**:
   ```bash
   npm run build:prod
   ```
   
   Or use the deploy script:
   ```bash
   npm run deploy
   ```

### Deployment Steps

1. **Build the project** using the command above
2. **Upload contents** of the `build` folder to your web server at `http://suptech.online/`
3. **Ensure server configuration** supports React Router (SPA routing)

### Server Configuration

#### Apache Server
- The `.htaccess` file is already included in the public folder
- It will be copied to the build folder automatically
- Ensures proper routing for React Router

#### Nginx Server
If using Nginx, add this configuration:
```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

### API Configuration

The application automatically detects the environment:

- **Development**: Uses `https://20.57.218.145:8080/api`
- **Production** (suptech.online): Uses `https://suptech.online/api`

### Environment Variables

The app uses these environment detection methods:
- `window.location.hostname` to detect if running on suptech.online
- `process.env.NODE_ENV` to detect development vs production

### Build Output

After running `npm run build:prod`, you'll get:
- Optimized production build in `build/` folder
- No source maps (for security)
- Compressed assets
- All static files ready for deployment

### Verification

After deployment, verify:
1. Navigate to `http://suptech.online/`
2. Check browser console for API URL logs
3. Test registration and login functionality
4. Verify all routes work correctly (refresh on any page should work)

### Troubleshooting

If you encounter issues:
1. Check browser console for API URL being used
2. Verify server supports SPA routing
3. Ensure API endpoints are accessible from suptech.online
4. Check network tab for failed requests

### File Structure After Build

```
build/
├── static/
│   ├── css/
│   ├── js/
│   └── media/
├── index.html
├── .htaccess (for Apache)
└── other static files
```

Upload all contents of the `build/` folder to your web server root directory.

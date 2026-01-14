# GitHub Pages Deployment Instructions

## Setup Steps

1. **Enable GitHub Pages** in your repository:
   - Go to Settings → Pages
   - Under "Build and deployment":
     - Source: GitHub Actions
   - Save the settings

2. **Push the changes**:
   ```bash
   git add .
   git commit -m "Add GitHub Pages deployment"
   git push origin main
   ```

3. **Monitor the deployment**:
   - Go to the "Actions" tab in your GitHub repository
   - You should see the "Deploy to GitHub Pages" workflow running
   - Once complete, your site will be available at:
     `https://hostilian.github.io/The-Desktop-Terrarium/`

## Files Created

- `.github/workflows/pages.yml` - GitHub Actions workflow for auto-deployment
- `docs/index.html` - Landing page HTML
- `docs/style.css` - Stylesheet for the landing page

## Notes

- The workflow automatically triggers on every push to `main` or `master`
- You can also manually trigger it from the Actions tab
- The workflow deploys the contents of the `docs/` folder

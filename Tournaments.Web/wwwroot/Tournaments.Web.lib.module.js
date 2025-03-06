// JavaScript initializer for Tournaments.Web
export function afterStarted(blazor) {
    console.log("Blazor application has started - initializing audio");
    
    // Resources to preload
    const resources = [
        { type: 'image', url: 'images/cs-background1.jpg' },
        { type: 'image', url: 'images/cs-background2.jpg' },
        { type: 'image', url: 'images/cs-background3.jpg' },
        { type: 'image', url: 'images/cs-background4.jpg' },
        { type: 'image', url: 'images/cs-background5.jpg' },
        { type: 'audio', url: 'audio/counterstrike.mp3' }
    ];
    
    // Track loading progress
    let loadedCount = 0;
    const totalResources = resources.length;
    
    // Function to update loading progress
    const updateProgress = () => {
        loadedCount++;
        const progressPercent = (loadedCount / totalResources) * 100;
        console.log(`Resource loading progress: ${progressPercent.toFixed(0)}%`);
        
        // If all resources are loaded, initialize audio
        if (loadedCount === totalResources) {
            initializeAudio();
        }
    };
    
    // Preload all resources
    resources.forEach(resource => {
        if (resource.type === 'image') {
            const img = new Image();
            img.onload = updateProgress;
            img.onerror = updateProgress; // Count errors as loaded to avoid blocking
            img.src = resource.url;
        } else if (resource.type === 'audio') {
            const audio = new Audio();
            audio.oncanplaythrough = updateProgress;
            audio.onerror = updateProgress; // Count errors as loaded to avoid blocking
            audio.src = resource.url;
            // Prevent actual playback during preload
            audio.preload = 'auto';
            audio.volume = 0;
        }
    });
    
    // Initialize audio player
    function initializeAudio() {
        // Create audio element
        var audio = document.createElement('audio');
        audio.id = 'background-music';
        audio.src = 'audio/counterstrike.mp3';
        audio.loop = true;
        audio.volume = 0.3;
        
        // Add to document and play
        document.body.appendChild(audio);
        
        // Try to play and handle any errors
        audio.play().catch(e => {
            console.log("Audio autoplay error:", e);
            
            // Set up a one-time click handler for the entire document
            const clickHandler = () => {
                audio.play().catch(err => console.log("Play on click failed:", err));
                document.removeEventListener('click', clickHandler);
            };
            
            document.addEventListener('click', clickHandler);
        });
        
        // Expose audio control functions globally
        window.audioControl = {
            play: function() {
                if (audio) {
                    audio.play().catch(e => console.log("Audio play error:", e));
                }
            },
            pause: function() {
                if (audio) {
                    audio.pause();
                }
            },
            isPlaying: function() {
                return audio ? !audio.paused : false;
            }
        };
    }
} 
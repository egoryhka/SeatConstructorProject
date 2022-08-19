var scene, camera, loader, renderer;
var hemilight, spotlight;

var loadedMods = [];
var currentSeat;

var floor;
var modelLoaded = false;

function recalcFloor() {

    scene.remove(floor);

    let box = new THREE.Box3();
    box.setFromObject(scene);

    scene.add(floor);
    floor.position.y = box.min.y - 0.01;
}

function init3Dscene(sceneContainer) {

    scene = new THREE.Scene();
    camera = new THREE.PerspectiveCamera(75, sceneContainer.offsetWidth / sceneContainer.offsetHeight);
    camera.position.set(-3, 0.5, -5);

    loader = new THREE.GLTFLoader();

    //Floor
    const geometry = new THREE.PlaneGeometry(5, 5);
    const material = new THREE.MeshBasicMaterial({ color: 0x777777, side: THREE.DoubleSide });
    material.transparent = true;
    material.opacity = 0.4;
    floor = new THREE.Mesh(geometry, material);
    floor.rotation.set(Math.PI / 2, 0, 0);

    //Light
    hemilight = new THREE.HemisphereLight(0xcccccc, 0x000000, 2);
    spotlight = new THREE.SpotLight(0xffffdc, 2, 10, 15);
    spotlight.position.set(0, 5, -2);

    scene.add(hemilight);
    scene.add(spotlight);

    //Helpers
    var spotLightHelper = new THREE.SpotLightHelper(spotlight);
    //scene.add(spotLightHelper)

    renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;


    renderer.setClearColor(0x000000, 0);
    renderer.setSize(sceneContainer.offsetWidth, sceneContainer.offsetHeight);
    sceneContainer.appendChild(renderer.domElement);

    var controls = new THREE.OrbitControls(camera, renderer.domElement);
    controls.enablePan = false;
    controls.target = new THREE.Vector3(0, 0, 0);
    controls.minDistance = 5.5;
    controls.maxDistance = 8;

    var animate = function () {
        //scene.rotation.y += 0.005;
        renderer.render(scene, camera);
        requestAnimationFrame(animate);
    }

    animate();

    window.addEventListener('resize', onWindowResize, false);

    function onWindowResize() {
        camera.aspect = sceneContainer.offsetWidth / sceneContainer.offsetHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(sceneContainer.offsetWidth, sceneContainer.offsetHeight);
    }

    recalcFloor();
}



function loadSeat(seat) {

    if (currentSeat != null) {
        if (seat.name == currentSeat.name) return false;
        scene.remove(currentSeat);
    }

    clearAllMods();
    modelLoaded = false;
    LoadingModelsImg.hidden = false;

    loader.load('/' + seat.pathTo3DModel,
        function (glb) {
            let object = glb.scene;

            object.traverse((node) => {
                if (node.isMesh) {
                    //node.castShadow = true;
                    //node.recieveShadow = true;

                    //if (node.material.map) node.material.map.anisotropy = 16;
                }
            });

            object.name = seat.name;
            object.position.y = -2.2;

            scene.add(object);
            currentSeat = object;
            modelLoaded = true;
            LoadingModelsImg.hidden = true;

            $({ n: 0.5 }).animate({ n: 1 }, {
                duration: 500,
                easing: 'easeOutBounce',
                step: function (now, fx) {
                    object.scale.set(now, now, now);
                    recalcFloor();
                }
            });

        },
        function (xhr) { /* console.log((xhr.loaded / xhr.total * 100) + "%loaded")*/ },
        function (error) { console.log(error) }
    );

    return true;
}

function removeMod(typeName) {
    var modObjectsOfType = where(loadedMods, function (modObject) { return modObject.name === typeName });
    for (var i = 0; i < modObjectsOfType.length; i++) {
        let modObject = modObjectsOfType[i];
        loadedMods.splice(loadedMods.indexOf(modObject), 1);
        scene.remove(modObject);
        recalcFloor();
    }
}

function loadMod(mod) {
    let typeName = mod.type.name;
    removeMod(typeName);

    if (mod == null) return;

    modelLoaded = false;
    LoadingModelsImg.hidden = false;

    loader.load('/' + mod.pathTo3DModel,
        function (glb) {
            let object = glb.scene;
            object.name = typeName;

            object.traverse((node) => {
                if (node.isMesh) {
                    //node.castShadow = true;
                    //node.recieveShadow = true;

                    //if (node.material.map) node.material.map.anisotropy = 16;
                }
            });

            object.position.x = mod.offsetX;
            object.position.y = mod.offsetZ;
            object.position.z = mod.offsetY;

            scene.add(object);
            loadedMods.push(object);
            modelLoaded = true;
            LoadingModelsImg.hidden = true;

            if (mod.mirror) {
                let mirroredObject = object.clone();
                mirroredObject.position.x = -mod.offsetX;
                mirroredObject.position.y = mod.offsetZ;
                mirroredObject.position.z = mod.offsetY;

                scene.add(mirroredObject);
                loadedMods.push(mirroredObject);

                $({ n: 0.5 }).animate({ n: 1 }, {
                    duration: 500,
                    easing: 'easeOutBounce',
                    step: function (now, fx) {
                        object.scale.set(now, now, now);
                        mirroredObject.scale.set(-now, now, now);
                        recalcFloor();
                    }
                });
            }
            else {
                $({ n: 0.5 }).animate({ n: 1 }, {
                    duration: 500,
                    easing: 'easeOutBounce',
                    step: function (now, fx) {
                        object.scale.set(now, now, now);
                        recalcFloor();
                    }
                });
            }

        },
        function (xhr) { /* console.log((xhr.loaded / xhr.total * 100) + "%loaded")*/ },
        function (error) { console.log(error) }
    );
}


function clearAllMods() {
    for (var i = 0; i < loadedMods.length; i++) {
        let modObject = loadedMods[i];
        scene.remove(modObject);
    }

    loadedMods = [];
    renderer.renderLists.dispose();

    recalcFloor();
}


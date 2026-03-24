// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";

contract AgentNFT is ERC721, ERC721Enumerable, AccessControl, ReentrancyGuard {
    bytes32 public constant MINTER_ROLE = keccak256("MINTER_ROLE");
    bytes32 public constant ELO_UPDATER_ROLE = keccak256("ELO_UPDATER_ROLE");
    bytes32 public constant BATTLE_OPERATOR_ROLE = keccak256("BATTLE_OPERATOR_ROLE");

    enum AgentClass { A, B, C, D, E }

    struct Agent {
        uint256 nftId;
        AgentClass agentClass;
        string modelName;
        uint256 eloRating;
        uint256 totalBattles;
        uint256 wins;
        uint256 stakedAmount;
        bool isActive;
    }

    mapping(AgentClass => uint256) public MAX_SUPPLY;
    uint256 private _nextTokenId;
    mapping(uint256 => Agent) private _agents;
    mapping(AgentClass => uint256) private _classMintCount;

    event AgentMinted(
        uint256 indexed nftId,
        address indexed owner,
        AgentClass agentClass
    );
    event EloUpdated(
        uint256 indexed nftId,
        uint256 oldElo,
        uint256 newElo
    );

    constructor() ERC721("AI Training Arena Agent", "AGENT") {
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _nextTokenId = 1;

        MAX_SUPPLY[AgentClass.A] = 15000;
        MAX_SUPPLY[AgentClass.B] = 6000;
        MAX_SUPPLY[AgentClass.C] = 2500;
        MAX_SUPPLY[AgentClass.D] = 1200;
        MAX_SUPPLY[AgentClass.E] = 300;
    }

    function mintAgent(
        address to,
        AgentClass agentClass,
        string calldata modelName
    ) external onlyRole(MINTER_ROLE) nonReentrant returns (uint256) {
        require(
            _classMintCount[agentClass] < MAX_SUPPLY[agentClass],
            "AgentNFT: class supply cap reached"
        );

        uint256 tokenId = _nextTokenId++;
        _classMintCount[agentClass]++;

        _agents[tokenId] = Agent({
            nftId: tokenId,
            agentClass: agentClass,
            modelName: modelName,
            eloRating: 1500,
            totalBattles: 0,
            wins: 0,
            stakedAmount: 0,
            isActive: true
        });

        _safeMint(to, tokenId);
        emit AgentMinted(tokenId, to, agentClass);
        return tokenId;
    }

    function updateElo(
        uint256 tokenId,
        uint256 newElo
    ) external onlyRole(ELO_UPDATER_ROLE) {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        uint256 oldElo = _agents[tokenId].eloRating;
        _agents[tokenId].eloRating = newElo;
        emit EloUpdated(tokenId, oldElo, newElo);
    }

    function incrementBattles(
        uint256 tokenId,
        bool won
    ) external onlyRole(BATTLE_OPERATOR_ROLE) {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        _agents[tokenId].totalBattles++;
        if (won) {
            _agents[tokenId].wins++;
        }
    }

    function setActive(uint256 tokenId, bool active) external {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        require(
            ownerOf(tokenId) == msg.sender ||
                hasRole(DEFAULT_ADMIN_ROLE, msg.sender),
            "AgentNFT: not owner or admin"
        );
        _agents[tokenId].isActive = active;
    }

    function getAgentClass(
        uint256 tokenId
    ) external view returns (AgentClass) {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        return _agents[tokenId].agentClass;
    }

    function getAgentElo(uint256 tokenId) external view returns (uint256) {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        return _agents[tokenId].eloRating;
    }

    function getUserAgents(
        address user
    ) external view returns (uint256[] memory) {
        uint256 balance = balanceOf(user);
        uint256[] memory tokens = new uint256[](balance);
        for (uint256 i = 0; i < balance; i++) {
            tokens[i] = tokenOfOwnerByIndex(user, i);
        }
        return tokens;
    }

    function getBattleCount(uint256 tokenId) external view returns (uint256) {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        return _agents[tokenId].totalBattles;
    }

    function isActive(uint256 tokenId) external view returns (bool) {
        require(_ownerOf(tokenId) != address(0), "AgentNFT: nonexistent token");
        return _agents[tokenId].isActive;
    }

    function _update(
        address to,
        uint256 tokenId,
        address auth
    ) internal override(ERC721, ERC721Enumerable) returns (address) {
        return super._update(to, tokenId, auth);
    }

    function _increaseBalance(
        address account,
        uint128 value
    ) internal override(ERC721, ERC721Enumerable) {
        super._increaseBalance(account, value);
    }

    function supportsInterface(
        bytes4 interfaceId
    ) public view override(ERC721, ERC721Enumerable, AccessControl) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}

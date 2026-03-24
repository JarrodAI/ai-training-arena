// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";

contract VestingSchedule is AccessControl, ReentrancyGuard {
    using SafeERC20 for IERC20;

    bytes32 public constant VESTING_ADMIN_ROLE = keccak256("VESTING_ADMIN_ROLE");
    bytes32 public constant ALLOCATOR_ROLE     = keccak256("ALLOCATOR_ROLE");

    enum VestingType { FOUNDER, NFT_BONUS }

    struct Schedule {
        uint256 totalAmount;
        uint256 claimedAmount;
        uint256 startTime;
        uint256 cliffDuration;
        uint256 vestDuration;
        VestingType vestingType;
        bool    revoked;
    }

    IERC20 public immutable ataToken;

    mapping(address => Schedule[]) public schedules;

    uint256 public totalReserved;

    event ScheduleCreated(address indexed beneficiary, uint256 scheduleIndex, uint256 totalAmount, VestingType vestingType);
    event Claimed(address indexed beneficiary, uint256 scheduleIndex, uint256 amount);
    event ScheduleRevoked(address indexed beneficiary, uint256 scheduleIndex, uint256 unvested);

    uint256 public constant FOUNDER_CLIFF   = 180 days;
    uint256 public constant FOUNDER_VEST    = 540 days;
    uint256 public constant NFT_CLIFF       =  90 days;
    uint256 public constant NFT_VEST        = 360 days;

    constructor(address _ataToken) {
        require(_ataToken != address(0), "VestingSchedule: zero ata");
        ataToken = IERC20(_ataToken);
        _grantRole(DEFAULT_ADMIN_ROLE,  msg.sender);
        _grantRole(VESTING_ADMIN_ROLE,  msg.sender);
        _grantRole(ALLOCATOR_ROLE,      msg.sender);
    }

    function createSchedule(
        address beneficiary,
        uint256 totalAmount,
        uint256 startTime,
        VestingType vestingType
    ) external onlyRole(ALLOCATOR_ROLE) {
        require(beneficiary  != address(0), "VestingSchedule: zero beneficiary");
        require(totalAmount  >  0,          "VestingSchedule: zero amount");
        require(startTime    >= block.timestamp, "VestingSchedule: start in past");

        (uint256 cliff, uint256 vest) = _durations(vestingType);

        schedules[beneficiary].push(Schedule({
            totalAmount:   totalAmount,
            claimedAmount: 0,
            startTime:     startTime,
            cliffDuration: cliff,
            vestDuration:  vest,
            vestingType:   vestingType,
            revoked:       false
        }));

        totalReserved += totalAmount;

        ataToken.safeTransferFrom(msg.sender, address(this), totalAmount);

        emit ScheduleCreated(beneficiary, schedules[beneficiary].length - 1, totalAmount, vestingType);
    }

    function claim(uint256 scheduleIndex) external nonReentrant {
        Schedule storage s = schedules[msg.sender][scheduleIndex];
        require(!s.revoked, "VestingSchedule: schedule revoked");

        uint256 vested    = _vestedAmount(s);
        uint256 claimable = vested - s.claimedAmount;
        require(claimable > 0, "VestingSchedule: nothing to claim");

        s.claimedAmount += claimable;
        totalReserved   -= claimable;

        ataToken.safeTransfer(msg.sender, claimable);

        emit Claimed(msg.sender, scheduleIndex, claimable);
    }

    function claimAll() external nonReentrant {
        Schedule[] storage mySchedules = schedules[msg.sender];
        uint256 totalClaimable;

        for (uint256 i = 0; i < mySchedules.length; i++) {
            Schedule storage s = mySchedules[i];
            if (s.revoked) continue;

            uint256 vested    = _vestedAmount(s);
            uint256 claimable = vested - s.claimedAmount;
            if (claimable == 0) continue;

            s.claimedAmount += claimable;
            totalReserved   -= claimable;
            totalClaimable  += claimable;

            emit Claimed(msg.sender, i, claimable);
        }

        require(totalClaimable > 0, "VestingSchedule: nothing to claim");
        ataToken.safeTransfer(msg.sender, totalClaimable);
    }

    function revoke(address beneficiary, uint256 scheduleIndex)
        external
        onlyRole(VESTING_ADMIN_ROLE)
        nonReentrant
    {
        Schedule storage s = schedules[beneficiary][scheduleIndex];
        require(!s.revoked, "VestingSchedule: already revoked");

        uint256 vested    = _vestedAmount(s);
        uint256 claimable = vested - s.claimedAmount;
        uint256 unvested  = s.totalAmount - vested;

        s.revoked = true;

        if (claimable > 0) {
            s.claimedAmount += claimable;
            totalReserved   -= claimable;
            ataToken.safeTransfer(beneficiary, claimable);
        }

        if (unvested > 0) {
            totalReserved -= unvested;
            ataToken.safeTransfer(msg.sender, unvested);
        }

        emit ScheduleRevoked(beneficiary, scheduleIndex, unvested);
    }

    function vestedAmount(address beneficiary, uint256 scheduleIndex)
        external
        view
        returns (uint256)
    {
        return _vestedAmount(schedules[beneficiary][scheduleIndex]);
    }

    function claimableAmount(address beneficiary, uint256 scheduleIndex)
        external
        view
        returns (uint256)
    {
        Schedule storage s = schedules[beneficiary][scheduleIndex];
        if (s.revoked) return 0;
        return _vestedAmount(s) - s.claimedAmount;
    }

    function scheduleCount(address beneficiary) external view returns (uint256) {
        return schedules[beneficiary].length;
    }

    function _vestedAmount(Schedule storage s) internal view returns (uint256) {
        if (block.timestamp < s.startTime + s.cliffDuration) {
            return 0;
        }
        uint256 elapsed = block.timestamp - s.startTime;
        uint256 linear  = s.vestDuration - s.cliffDuration;
        if (elapsed >= s.vestDuration) {
            return s.totalAmount;
        }
        uint256 elapsedAfterCliff = elapsed - s.cliffDuration;
        return (s.totalAmount * elapsedAfterCliff) / linear;
    }

    function _durations(VestingType t)
        internal
        pure
        returns (uint256 cliff, uint256 vest)
    {
        if (t == VestingType.FOUNDER) {
            return (FOUNDER_CLIFF, FOUNDER_VEST);
        }
        return (NFT_CLIFF, NFT_VEST);
    }
}
